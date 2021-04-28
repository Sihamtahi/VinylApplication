using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using AirVinyl.DataAccessLayer;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using AirVinyl.API.Helpers;
 

namespace AirVinylAPI.Controllers
{
    public class RecordStoresController : ODataController
    {


        private AirVinylDbContext _ctx = new AirVinylDbContext();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            //this will creat an http response with code 200 ! and the list of VinylRecords.
            return Ok(_ctx.RecordStores);
        }


        [EnableQuery]
        public IHttpActionResult GetRecordStoreKey([FromODataUri] int key)
        {
            var recordStrores  = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key);
            if (recordStrores == null)
            {
                return NotFound();
            }
            return Ok(recordStrores);
        }



        [HttpGet]
        [ODataRoute("RecordStores({key})/Tags")]
        public IHttpActionResult GetRecordStoresPropoerty([FromODataUri] int key)
        {

            /// No include necessary cuz Tags is not a navigation propoerty
            var recordStores = _ctx.RecordStores.FirstOrDefault(p => p.RecordStoreId == key);
            if (recordStores == null)
            {
                return NotFound();
            }
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            var collectionPropertyValue = recordStores.GetValue(collectionPropertyToGet);
             // return the collection *-*
            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }



        protected override void Dispose(bool disposing)
        {

            _ctx.Dispose();
            base.Dispose(disposing);
        }


    }





}
