using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Connect.DNN.Modules.Conference.Common;
using Newtonsoft.Json;
using Connect.Conference.Core.Repositories;
using System.Collections.Generic;
using System.Web;

namespace Connect.DNN.Modules.Conference.Api
{

    public partial class LocationsController : ConferenceApiController
    {

        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage List(int conferenceId)
        {
            return Request.CreateResponse(HttpStatusCode.OK, LocationRepository.Instance.GetLocationsByConference(conferenceId).OrderBy(l => l.Sort));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.ManageConference)]
        public HttpResponseMessage Reorder(int conferenceId)
        {
            var raw = new System.IO.StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();
            var data = JsonConvert.DeserializeObject<List<Order>>(raw);
            ILocationRepository _repository = LocationRepository.Instance;
            foreach (Order no in data)
            {
                var location = _repository.GetLocation(conferenceId, no.id);
                if (location != null)
                {
                    location.Sort = no.order;
                    _repository.UpdateLocation(location.GetLocationBase(), UserInfo.UserID);
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.ManageConference)]
        public HttpResponseMessage Delete(int conferenceId, int id)
        {
            ILocationRepository _repository = LocationRepository.Instance;
            _repository.DeleteLocation(conferenceId, id);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage Get(int conferenceId, int locationId)
        {
            return Request.CreateResponse(HttpStatusCode.OK, LocationRepository.Instance.GetLocation(conferenceId, locationId));
        }

    }
}

