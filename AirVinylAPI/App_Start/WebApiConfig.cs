using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AirVinyl.Model;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;

namespace AirVinylAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Configuration et services API Web
           
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            config.MapODataServiceRoute(
                                        routeName: "ODataRoute",
                                        routePrefix: "odata",
                                        model: GetEdmModel());


            config.EnsureInitialized();



        }

        private static IEdmModel GetEdmModel()
        {
            var  builder = new ODataConventionModelBuilder();
            /// auto mapping ?
            builder.Namespace = "AirVinyl";
            builder.ContainerName = "AirVinylContainer";
            builder.EntitySet<Person>("People"); 
        ///    builder.EntitySet<VinylRecord>("VinylRecords");
            builder.EntitySet<RecordStore>("RecordStores");

            return builder.GetEdmModel();
             
        }
    }
}
