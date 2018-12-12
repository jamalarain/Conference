using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using Connect.DNN.Modules.Conference.Common;
using Connect.Conference.Core.Repositories;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using Connect.Conference.Core.Models.SessionEvaluations;
using System;
using System.Text.RegularExpressions;

namespace Connect.DNN.Modules.Conference.Api
{

    public partial class SessionAttendeesController : ConferenceApiController
    {

        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.AttendConference)]
        public HttpResponseMessage Attendances(int conferenceId)
        {
            var res = SessionAttendeeRepository.Instance.GetSessionAttendeesByUser(conferenceId, UserInfo.UserID);
            return Request.CreateResponse(HttpStatusCode.OK, res);
        }

        [HttpGet]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.AttendConference)]
        public HttpResponseMessage SessionAttendees(int conferenceId, int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, SessionAttendeeRepository.Instance.GetSessionAttendeesBySession(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.AttendConference)]
        public HttpResponseMessage Set(int conferenceId, SessionEvaluationBase data)
        {
            var attended = SessionAttendeeRepository.Instance.GetSessionAttendeesByUser(UserInfo.UserID).FirstOrDefault(a => a.SessionId == data.SessionId);
            if (attended != null)
            {
                data.UserId = UserInfo.UserID;
                SessionEvaluationRepository.Instance.SetSessionEvaluation(data, UserInfo.UserID);
                return Request.CreateResponse(HttpStatusCode.OK, "");
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Attendee did not attend this session");
            }
        }

        public class AttendByCodeDTO
        {
            public int SessionId { get; set; }
            public string Codes { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ConferenceAuthorize(SecurityLevel = SecurityAccessLevel.ManageConference)]
        public HttpResponseMessage AttendByCode(int conferenceId, AttendByCodeDTO data)
        {
            var lines = GetCodes(data.Codes);
            foreach (var code in lines)
            {
                if (!String.IsNullOrEmpty(code))
                {
                    var attendee = AttendeeRepository.Instance.GetAttendeeByCode(conferenceId, code);
                    if (attendee != null)
                    {
                        SessionAttendeeRepository.Instance.SetSessionAttendee(data.SessionId, attendee.UserId);
                    }
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, SessionAttendeeRepository.Instance.GetSessionAttendeesBySession(data.SessionId));
        }

        private List<string> GetCodes(string rawstring)
        {
            var res = new List<string>();
            var lines = rawstring.Split('\n').Where(l => !string.IsNullOrEmpty(l)).ToArray();
            var line = 0;
            var m = Regex.Match(lines[line], "Total Counters = (\\d+)");
            if (m.Success)
            {
                var count = int.Parse(m.Groups[1].Value);
                if (lines.Count() != count + 1)
                {
                    throw new Exception("Nr codes does not equal total specified");
                }
                line++;
            }
            while (line < lines.Count())
            {
                var code = lines[line];
                var lm = Regex.Match(code, "No\\.:(\\d+)");
                if (lm.Success)
                {
                    code = lm.Groups[1].Value;
                }
                res.Add(code);
                line++;
            }
            return res;
        }
    }
}
