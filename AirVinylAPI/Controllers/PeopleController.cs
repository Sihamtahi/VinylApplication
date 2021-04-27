using AirVinyl.DataAccessLayer;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using AirVinyl.API.Helpers;
namespace AirVinylAPI.Controllers
{
    public class PeopleController : ODataController 
    {

        private AirVinylDbContext _ctx = new AirVinylDbContext();

        public   IHttpActionResult Get ()
        {
            //this will creat an http response with code 200 ! and the list of people.
            return Ok(_ctx.People);
        }

        public IHttpActionResult Get([FromODataUri] int key )
        {
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if ( person == null)
            {
                return NotFound();
            }
            return Ok(person);
        }

        [HttpGet]
        [ODataRoute("People({key})/Email")]
        [ODataRoute("People({key})/FirstName")]
        [ODataRoute("People({key})/LastName")]
        [ODataRoute("People({key})/Gender")]
        public IHttpActionResult GetPersonPropoerty([FromODataUri]  int key)
        {
            
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (person == null)
            {
                return NotFound();
            }


            var propertyToGet = Url.Request.RequestUri.Segments.Last();

            if (!person.HasProperty(propertyToGet))
            {
                return NotFound();
            }
            var propertyValue = person.GetValue(propertyToGet);
            if ( propertyValue == null )
            {
                return StatusCode(System.Net.HttpStatusCode.NoContent);
            }
            return this.CreateOKHttpActionResult(propertyValue);
        }


        [HttpGet]
        [ODataRoute("People({key})/Friends")]
        [ODataRoute("People({key})/VinylRecords")]
        [ODataRoute("People({key})/LastName")]
        [ODataRoute("People({key})/Gender")]
        public IHttpActionResult GetPersonCollectionPropoerty([FromODataUri] int key)

        { 

            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            var person = _ctx.People.Include(collectionPropertyToGet)
                .FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }
            
            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);
             
            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        public IHttpActionResult Post(AirVinyl.Model.Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _ctx.People.Add(person);
            _ctx.SaveChanges();

            return Created(person);
        }



        public IHttpActionResult Put( [FromODataUri] int key ,AirVinyl.Model.Person person)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if ( currentPerson == null )
            {
                return NotFound();
            }

            /// to make sure that we are nt updating an other person 

            person.PersonId = currentPerson.PersonId;
            _ctx.Entry(currentPerson).CurrentValues.SetValues(person);
            _ctx.SaveChanges();
            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        public IHttpActionResult Patch([FromODataUri] int key, Delta<AirVinyl.Model.Person> patch)
        {   //Delta is a class used to track changes in an EntityType
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var currentPerson = _ctx.People.FirstOrDefault(p => p.PersonId == key);
            if (currentPerson == null)
            {
                return NotFound();
            }






            patch.Patch(currentPerson);
            _ctx.SaveChanges();
            return StatusCode(System.Net.HttpStatusCode.NoContent);


        }

        public IHttpActionResult Delete ([FromODataUri] int key)
        {

            var currentPerson = _ctx.People.Include("Friends").FirstOrDefault(p => p.PersonId == key);
            if ( currentPerson == null )
            {
                return NotFound();
            }
            var peopleWithCurrentPersonFriend = _ctx.People.Include("Friends")
                .Where(p => p.Friends.Select(f => f.PersonId).AsQueryable().Contains(key));


            foreach(var person in peopleWithCurrentPersonFriend.ToList())
            {

                person.Friends.Remove(currentPerson);
            }

           return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {

            _ctx.Dispose();
            base.Dispose(disposing);
        }





    }
}