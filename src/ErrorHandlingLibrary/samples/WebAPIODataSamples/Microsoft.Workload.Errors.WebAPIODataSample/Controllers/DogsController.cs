//---------------------------------------------------------------------
// <copyright file="DogsController.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved.
// </copyright>
//---------------------------------------------------------------------

namespace Microsoft.Workload.Errors.WebAPIODataSample.Controllers
{
    using System.Linq;
    using System.Web.Http;
    using Microsoft.AspNet.OData;
    using Microsoft.Workload.Errors.WebAPIODataSample.ErrorHandling;
    using Microsoft.Workload.Errors.WebAPIODataSample.Models;

    public class DogsController : ODataController
    {
        private DogContext database = new DogContext();

        public DogsController()
        {
            if (!database.Dogs.Any())
            {
                database.Dogs.AddRange(SampleData.GetDogs());
                database.SaveChanges();
            }
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(database.Dogs);
        }

        [EnableQuery]
        public IHttpActionResult Get(int key)
        {
            // Contrived example of not allowing a specific Dog to be accessed
            if (key == 5)
            {
                // Assume that there was auth-related code that resolves to an exception.
                throw new AccessDeniedException(
                    Utilities.Constants.ErrorCodeOwnershipRestriction,
                    string.Format("Access to Dog({0}) is restricted to only its owner.", key));
            }

            return Ok(database.Dogs.FirstOrDefault(dog => dog.Id == key));
        }

        [HttpPost]
        public IHttpActionResult Post(Dog dog)
        {
            database.Dogs.Add(dog);
            database.SaveChanges();
            return Created(dog);
        }

        protected override void Dispose(bool disposing)
        {
            database.Dispose();
            base.Dispose(disposing);
        }
    }
}