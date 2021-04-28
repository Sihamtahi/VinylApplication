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
       
        [ODataRoute]
        [EnableQuery]
        public   IHttpActionResult Get ()
        {
            //this will creat an http response with code 200 ! and the list of people.
            return Ok(_ctx.People);
        }
        [EnableQuery]
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
        [EnableQuery]
        public IHttpActionResult GetPersonCollectionPropoerty([FromODataUri] int key)

        { 
            /// Dans ce cas, nous allons récuperer tout l'enregistrement de 
            /// Person 
            /// par la suite seuls les champs sollicités seront retournée
            /// Donc on accède à tous les champs de l'entité person 


            //on récupère le
            var collectionPropertyToGet = Url.Request.RequestUri.Segments.Last();
            // récupère la personne ayant l'id = key avec un iclude de la Property
            var person = _ctx.People.Include(collectionPropertyToGet)
                .FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }

            //On retourne la colection d'éléments de property
            
            var collectionPropertyValue = person.GetValue(collectionPropertyToGet);
             
            return this.CreateOKHttpActionResult(collectionPropertyValue);
        }

        /**********Récupérer direcetement le champs voulu dans la BDD **************/

        [HttpGet]
        [ODataRoute("People({key})/VinylRecords")]
        [EnableQuery]
        public IHttpActionResult GetVinylRecordsForPerson([FromODataUri] int key)

        {

            // On verifie juste si la personne contenant l'id key existe dans la BDD
            var person = _ctx.People.FirstOrDefault(p => p.PersonId == key);

            if (person == null)
            {
                return NotFound();
            }
            //nous renovoyons les VinylRecords uniquement pour la personne
            // Pas la peine de récuperer toute la personne.
            return  Ok(_ctx.VinylRecords.Where(v=> v.Person.PersonId == key);
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

            _ctx.People.Remove(currentPerson);
            _ctx.SaveChanges();
           return StatusCode(System.Net.HttpStatusCode.NoContent);
        }




        [HttpPost]
        [ODataRoute("People({key})/Friends/$ref")] 
  
        public IHttpActionResult CreateLinkToFriend([FromODataUri] int key, [FromBody] Uri link)
        {            
            var currentperson = _ctx.People.Include("Friends")
                .FirstOrDefault(p => p.PersonId == key);

            if (currentperson == null)
            {
                return NotFound();
            }

            int keyOfFriendToAadd = Request.GetKeyValue<int>(link);
            if (currentperson.Friends.Any(i => i.PersonId == keyOfFriendToAadd))
            {
                return BadRequest(String.Format("the person with the is {0 } is already linked to the person with the id {1}", key,keyOfFriendToAadd));
            }

            var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfFriendToAadd);
            if (friendToLinkTo == null)
            {
                return NotFound();
            }
            currentperson.Friends.Add(friendToLinkTo);
            _ctx.SaveChanges();

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }


        [HttpPost]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]

        public IHttpActionResult UpadateLinkToFriend([FromODataUri] int key, [FromODataUri] int realtedKey, [FromBody] Uri link)
        {

            //find th person if exists , return NotFound if not
            var currentperson = _ctx.People.Include("Friends")
                .FirstOrDefault(p => p.PersonId == key);

            if (currentperson == null)
            {
                return NotFound();
            }
            //search the friend 
            var currentFriend = currentperson.Friends.FirstOrDefault(f => f.PersonId == realtedKey);
            if ( currentFriend == null)
            {
                return NotFound();
            }


            int keyOfTheFriendToAdd = Request.GetKeyValue<int>(link);
            if (currentperson.Friends.Any(i => i.PersonId == keyOfTheFriendToAdd))
            {
                return BadRequest(String.Format("the person with the is {0 } is already linked to the person with the id {1}", key, keyOfTheFriendToAdd));
            }

           
            var friendToLinkTo = _ctx.People.FirstOrDefault(p => p.PersonId == keyOfTheFriendToAdd);
            if (friendToLinkTo == null)
            {
                return NotFound();
            }
            currentperson.Friends.Remove(currentFriend);
            currentperson.Friends.Add(friendToLinkTo);
            _ctx.SaveChanges();

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }



        //Delete the relationship
        // DELETE odata/People(key)/Freinds/$re$id ={relatedUriWithRelatedKey}
        [HttpPost]
        [ODataRoute("People({key})/Friends({relatedKey})/$ref")]

        public IHttpActionResult DeleteLinkToFriend([FromODataUri] int key, [FromODataUri] int realtedKey, [FromBody] Uri link)
        {
            /// d'abord on verfie si la person et son ami existent 
            /// Par la suite on supprime l'ami de la liste des ami de la personne
            //find th person if exists , return NotFound if not
            var currentperson = _ctx.People.Include("Friends")
                .FirstOrDefault(p => p.PersonId == key);

            if (currentperson == null)
            {
                return NotFound();
            }
            //search the friend 
            var friend = currentperson.Friends.FirstOrDefault(f => f.PersonId == realtedKey);
            if (friend == null)
            {
                return NotFound();
            }


            int keyOfTheFriendToAdd = Request.GetKeyValue<int>(link);
            if (currentperson.Friends.Any(i => i.PersonId == keyOfTheFriendToAdd))
            {
                return BadRequest(String.Format("the person with the is {0} is already linked to the person with the id {1}", key, keyOfTheFriendToAdd));
            }            
            currentperson.Friends.Remove(friend);
            _ctx.SaveChanges();
            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }











        protected override void Dispose(bool disposing)
        {

            _ctx.Dispose();
            base.Dispose(disposing);
        }


    }
}