using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Connect.DNN.Modules.Conference.Common;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using Connect.Conference.Core.Repositories;
using System.Net.Http.Headers;
using Connect.Conference.Core.Models.Sessions;
using Connect.Conference.Core.Models.SessionAttendees;
using Connect.Conference.Core.Common;

namespace Connect.DNN.Modules.Conference.Api
{

    public partial class SessionsController : ConferenceApiController
    {
        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage List(int conferenceId)
        {
            return Request.CreateResponse(HttpStatusCode.OK, SessionRepository.Instance.GetSessionsByConference(conferenceId).Where(s => s.Status.UnNull(0) >= (int)SessionStatus.Accepted).OrderBy(s => s.Title));
        }

        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Get(int conferenceId, int sessionId)
        {
            var res = SessionRepository.Instance.GetSession(sessionId);
            if (res.ConferenceId == conferenceId)
            {
                return Request.CreateResponse(HttpStatusCode.OK, res);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Bad request");
            }
        }

        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Agenda(int conferenceId)
        {
            var res = SessionRepository.Instance.GetSessionsByConference(conferenceId).Where(s => s.Status > 2);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Schedule(int conferenceId)
        {
            return Request.CreateResponse(HttpStatusCode.OK, Connect.Conference.Core.Models.Schedule.Create(PortalSettings.PortalId, conferenceId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.ManageConference)]
        public HttpResponseMessage Reorder(int conferenceId)
        {
            var raw = new System.IO.StreamReader(HttpContext.Current.Request.InputStream).ReadToEnd();
            var data = JsonConvert.DeserializeObject<List<Order>>(raw);
            ISessionRepository _repository = SessionRepository.Instance;
            foreach (Order no in data)
            {
                var session = _repository.GetSession(conferenceId, no.id);
                if (session != null)
                {
                    session.Sort = no.order;
                    _repository.UpdateSession(session.GetSessionBase(), UserInfo.UserID);
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        public class voteDTO
        {
            public int vote { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.Authenticated)]
        public HttpResponseMessage Vote(int conferenceId, int id, [FromBody]voteDTO vote)
        {
            if (vote.vote == 1)
            {
                SessionVoteRepository.Instance.SetSessionVote(id, UserInfo.UserID);
            }
            else
            {
                SessionVoteRepository.Instance.DeleteSessionVote(id, UserInfo.UserID);
            }
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        public class moveDTO
        {
            public int Day { get; set; }
            public int SlotId { get; set; }
            public int LocationId { get; set; }
            public bool DisplaceOthers { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.ManageConference)]
        public HttpResponseMessage Move(int conferenceId, int id, [FromBody]moveDTO moveParams)
        {
            var session = SessionRepository.Instance.GetSession(conferenceId, id);
            var speakerIds = session.Speakers.Select(s => s.Key).ToList();
            if (session == null)
            {
                return ServiceError("Can't find session");
            }
            if (session.SlotId == moveParams.SlotId & session.DayNr == moveParams.Day & session.LocationId == moveParams.LocationId)
            {
                return ServiceError("Not moved");
            }
            var parallelSessions = SessionRepository.Instance.GetSessionsBySlot(conferenceId, moveParams.Day, moveParams.SlotId);
            foreach (var s in parallelSessions)
            {
                if (id != s.SessionId)
                {
                    foreach (var sp in s.Speakers.Select(sp => sp.Key))
                    {
                        if (speakerIds.Contains(sp))
                        {
                            return ServiceError("This move would cause a speaker to have to be in 2 places at the same time. Please revise.");
                        }
                    }
                }
            }
            if (moveParams.DisplaceOthers)
            {
                if (session.IsPlenary)
                {
                    foreach (var s in parallelSessions)
                    {
                        s.SlotId = 0;
                        s.LocationId = null;
                        s.DayNr = 0;
                        SessionRepository.Instance.UpdateSession(s.GetSessionBase(), UserInfo.UserID);
                    }
                }
                foreach (var s in parallelSessions)
                {
                    if (s.LocationId == moveParams.LocationId)
                    {
                        s.SlotId = 0;
                        s.LocationId = null;
                        s.DayNr = 0;
                        SessionRepository.Instance.UpdateSession(s.GetSessionBase(), UserInfo.UserID);
                    }
                }
            }
            else
            {
                if (session.IsPlenary & parallelSessions.Count() != 0)
                {
                    return ServiceError("This plenary session would collide with other sessions. Please remove parallel sessions first.");
                }
                foreach (var s in parallelSessions)
                {
                    if (s.LocationId == moveParams.LocationId)
                    {
                        return ServiceError("This plenary session would collide with another sessions. Please remove existing session first.");
                    }
                }
            }
            session.SlotId = moveParams.SlotId;
            if (!session.IsPlenary)
            {
                session.LocationId = moveParams.LocationId;
            }
            session.DayNr = moveParams.Day;
            SessionRepository.Instance.UpdateSession(session.GetSessionBase(), UserInfo.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, SessionRepository.Instance.GetSessionsByConference(conferenceId).Where(s => s.Status > 2).OrderBy(s => s.Title));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.ManageConference)]
        public HttpResponseMessage Remove(int conferenceId, int id)
        {
            var session = SessionRepository.Instance.GetSession(conferenceId, id);
            if (session == null)
            {
                return ServiceError("Can't find session");
            }
            session.SlotId = 0;
            session.LocationId = null;
            session.DayNr = 0;
            SessionRepository.Instance.UpdateSession(session.GetSessionBase(), UserInfo.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, SessionRepository.Instance.GetSessionsByConference(conferenceId).Where(s => s.Status > 2).OrderBy(s => s.Title));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.SessionSubmit)]
        public HttpResponseMessage Delete(int conferenceId, int id)
        {
            var session = SessionRepository.Instance.GetSession(conferenceId, id);
            if (session == null)
            {
                return ServiceError("Can't find session");
            }
            if (!ConferenceModuleContext.Security.CanManage)
            {
                if (!ConferenceModuleContext.Security.IsPresenter(id))
                {
                    return AccessViolation("You can't delete this session because you are not a presenter of it");
                }
            }
            SessionRepository.Instance.DeleteSession(session.ConferenceId, session.SessionId);
            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        public class StatusDTO
        {
            public int newStatus { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.SessionSubmit)]
        public HttpResponseMessage ChangeStatus(int conferenceId, int id, [FromBody]StatusDTO data)
        {
            var session = SessionRepository.Instance.GetSession(conferenceId, id);
            if (session == null)
            {
                return ServiceError("Can't find session");
            }
            if (!ConferenceModuleContext.Security.CanManage)
            {
                if (!ConferenceModuleContext.Security.IsPresenter(id))
                {
                    return AccessViolation("You can't delete this session because you are not a presenter of it");
                }
            }
            session.Status = data.newStatus;
            SessionRepository.Instance.UpdateSession(session.GetSessionBase(), UserInfo.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, SessionRepository.Instance.GetSession(session.ConferenceId, session.SessionId));
        }

        public class TrackDTO
        {
            public int newTrack { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.ManageConference)]
        public HttpResponseMessage ChangeTrack(int conferenceId, int id, [FromBody]TrackDTO data)
        {
            var session = SessionRepository.Instance.GetSession(conferenceId, id);
            if (session == null)
            {
                return ServiceError("Can't find session");
            }
            session.TrackId = data.newTrack;
            SessionRepository.Instance.UpdateSession(session.GetSessionBase(), UserInfo.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, SessionRepository.Instance.GetSession(session.ConferenceId, session.SessionId));
        }

        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.ManageConference)]
        public HttpResponseMessage Download(int conferenceId)
        {
            var res = new HttpResponseMessage(HttpStatusCode.OK);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Title,SubTitle,Speakers,Status,Tags,Level,Track,Location,Votes,Plenary");
            foreach (var session in SessionRepository.Instance.GetSessionsByConference(conferenceId).OrderBy(s => s.Title))
            {
                sb.AppendLine(string.Format("\"{0}\",\"{1}\",\"{2}\",{3},\"{4}\",\"{5}\",\"{6}\",\"{7}\",{8},{9}", session.Title, session.SubTitle, string.Join(", ", session.Speakers.Select(sp => sp.Value)), session.Status, string.Join(", ", session.Tags.Select(t => t.Value)), session.Level, session.TrackTitle, session.LocationName, session.NrVotes, session.IsPlenary));
            }
            res.Content = new StringContent(sb.ToString());
            res.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            res.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            res.Content.Headers.ContentDisposition.FileName = "Sessions.csv";
            return res;
        }

        public class NextResponse
        {
            public Dictionary<int, IEnumerable<Session>> Sessions { get; set; }
            public Dictionary<int, IEnumerable<SessionAttendee>> Attendees { get; set; }
        }


        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage Next(int conferenceId)
        {
            var res = new NextResponse();
            res.Sessions = new Dictionary<int, IEnumerable<Session>>();
            res.Attendees = new Dictionary<int, IEnumerable<SessionAttendee>>();
            var rooms = LocationRepository.Instance.GetLocationsByConference(conferenceId);
            var sessions = SessionRepository.Instance.GetSessionsByConference(conferenceId).Where(s => s.SessionEnd > System.DateTime.Now & s.SessionDateAndTime != null && ((System.DateTime)s.SessionDateAndTime).Date == System.DateTime.Now.Date);
            var attendees = SessionAttendeeRepository.Instance.GetSessionAttendees(conferenceId);
            foreach (var room in rooms)
            {
                res.Sessions.Add(room.LocationId, sessions.Where(s => s.LocationId == room.LocationId).OrderBy(s => s.SessionDateAndTime));
            }
            foreach (var session in sessions)
            {
                res.Attendees.Add(session.SessionId, attendees.Where(a => a.SessionId == session.SessionId).OrderBy(a => a.SessionAttendeeName));
            }
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }
    }
}

