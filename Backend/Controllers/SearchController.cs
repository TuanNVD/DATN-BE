using System.Security.Cryptography;
using Backend.Context;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO;
using NetTopologySuite;
using Npgsql;

namespace Backend.Controllers
{
    [Route("api/v1")]
    [ApiController]
    public class SearchController : Controller
    {
        private readonly IConfiguration _config;
        public SearchController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("place/search")]
        public IActionResult PlaceSearch(string place)
        {
            if (place == null)
            {
                return BadRequest(new { message = "No parameters" });
            }
            string[] placeParts = place.Split(' ');
            string q = placeParts[0];
            string q1 = "", q2 = "";
            if (q == "tỉnh")
            {
                q1 = "city";
                q2 = "state";
                place = string.Join(" ", placeParts, 1, placeParts.Length - 1);
            } else if (q == "huyện" || q == "quận")
            {
                q1 = "suburb";
                q2 = "town";
                place = string.Join(" ", placeParts, 1, placeParts.Length - 1);
            } else
            {
                q1 = "%%"; q2 = "%%";
            }
                

            using var conn = new NpgsqlConnection(_config["ConnectionStrings:DefaultConnection"]);
            conn.Open();
            string query = $"select name, ST_AsText(ST_Transform(way, 4326)) from planet_osm_point where name ilike '%{place}%' and (place like '{q1}' or place like '{q2}')";
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            var wktGeometry = reader.GetString(1);

                            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory();
                            var geometry = new WKTReader(geometryFactory).Read(wktGeometry);

                            if (geometry is NetTopologySuite.Geometries.Point point)
                            {
                                var latitude = point.Y;
                                var longitude = point.X;

                                return Ok(new { lat = latitude, lon = longitude });
                            }
                            else
                            {
                                return BadRequest(new { message = "Unsupported geometry type" });
                            }
                        }
                        else
                        {
                            return NotFound(new { message = "Place not found" });
                        }
                    }
                    else
                    {
                        return NotFound(new { message = "Place not found" });
                    }
                }
            }
        }

        [HttpGet("test")]
        public IActionResult Test(string place)
        {
            if (place == null)
            {
                return BadRequest(new { message = "No parameters" });
            }
            using var conn = new NpgsqlConnection(_config["ConnectionStrings:DefaultConnection"]);
            conn.Open();
            string query = $"select ST_AsText(ST_Transform(geom, 4326)) from osm.boundary where uppername like upper('%{place}%')";
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            var wktGeometry = reader.GetString(0);

                            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory();
                            var geometry = new WKTReader(geometryFactory).Read(wktGeometry);

                            if (geometry is NetTopologySuite.Geometries.MultiPolygon multipolygon)
                            {
                                var latLngs = new List<List<List<double>>>();
                                foreach (var polygon in multipolygon.Geometries)
                                {
                                    var polygonLatLngs = new List<List<double>>();
                                    foreach (var coordinate in polygon.Coordinates)
                                    {
                                        polygonLatLngs.Add(new List<double> { coordinate.Y, coordinate.X });
                                    }
                                    latLngs.Add(polygonLatLngs);
                                }

                                return Ok(new { multipolygon = latLngs });
                            }
                            else if (geometry is NetTopologySuite.Geometries.Polygon polygon)
                            {
                                var latLngs = new List<List<double>>();
                                foreach (var coordinate in polygon.Coordinates)
                                {
                                    latLngs.Add(new List<double> { coordinate.Y, coordinate.X });
                                }

                                return Ok(new { polygon = latLngs });
                            }                      
                            else
                            {
                                return BadRequest(new { message = "Unsupported geometry type" });
                            }
                        }
                        else
                            return NotFound(new { message = "Place not found" });
                    }
                    else
                    {
                        return NotFound(new { message = "Place not found" });
                    }
                }
            }
        }
    }
}
