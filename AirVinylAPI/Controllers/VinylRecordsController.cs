using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using AirVinyl.DataAccessLayer;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using AirVinyl.API.Helpers;

/****
namespace AirVinylAPI.Controllers
{
    public class VinylRecordsController :  ODataController
    {

        private AirVinylDbContext _ctx = new AirVinylDbContext();
         [HttpGet]
         [ODataRoute("VinylRecords")]
        public IHttpActionResult GetAllVinylRecords()
        {
            //this will creat an http response with code 200 ! and the list of VinylRecords.
            return Ok(_ctx.VinylRecords);
        }


        [HttpGet]
        [ODataRoute("VinylRecords({key})")]
        public IHttpActionResult GetOneVinylRecord([FromODataUri] int key)
        {
            var vinylRecord = _ctx.VinylRecords.FirstOrDefault(p => p.VinylRecordId == key);
            if (vinylRecord == null)
            {
                return NotFound();
            }
            return Ok(vinylRecord);
        }


        protected override void Dispose(bool disposing)
        {

            _ctx.Dispose();
            base.Dispose(disposing);
        }
        

    }
}
**/