using System.Collections.Generic;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using Connect.Conference.Core.Models.Speakers;

namespace Connect.Conference.Core.Repositories
{

    public partial class SpeakerRepository : ServiceLocator<ISpeakerRepository, SpeakerRepository>, ISpeakerRepository
    {
        public IEnumerable<SpeakerWithNrSessions> GetSpeakersByConference(int conferenceId)
        {
            using (var context = DataContext.Instance())
            {
                return context.ExecuteQuery<SpeakerWithNrSessions>(System.Data.CommandType.Text,
                    "SELECT sp.*," +
                    " (SELECT COUNT(*) FROM {databaseOwner}{objectQualifier}Connect_Conference_Sessions s" +
                    "  INNER JOIN {databaseOwner}{objectQualifier}Connect_Conference_SessionSpeakers ss ON ss.SessionId=s.SessionId" +
                    "  WHERE s.ConferenceId=@0 AND ss.SpeakerId=sp.UserId AND s.Status>2) NrSessions" +
                    " FROM {databaseOwner}{objectQualifier}vw_Connect_Conference_Speakers sp WHERE sp.ConferenceId=@0",
                    conferenceId);
            }
        }
        public IEnumerable<SpeakerWithNrSessions> GetSpeakersByConferenceWithNrSessions(int conferenceId, int sessionStatusThreshold)
        {
            using (var context = DataContext.Instance())
            {
                return context.ExecuteQuery<SpeakerWithNrSessions>(System.Data.CommandType.Text,
                    "SELECT sp.*, (SELECT COUNT(*) FROM {databaseOwner}{objectQualifier}Connect_Conference_SessionSpeakers ss INNER JOIN {databaseOwner}{objectQualifier}Connect_Conference_Sessions s ON s.SessionId=ss.SessionId WHERE ss.SpeakerId=sp.UserId AND s.ConferenceId=@0 AND s.Status>=@1) NrSessions FROM {databaseOwner}{objectQualifier}vw_Connect_Conference_Speakers sp WHERE sp.ConferenceId=@0",
                    conferenceId, sessionStatusThreshold);
            }
        }
        public IEnumerable<DnnUser> SearchUsers(int portalId, string search)
        {
            using (var context = DataContext.Instance())
            {
                return context.ExecuteQuery<DnnUser>(System.Data.CommandType.Text, "SELECT u.* FROM {databaseOwner}{objectQualifier}vw_Users u WHERE u.PortalId=@0 AND (u.FirstName LIKE '%' + @1 + '%' OR u.LastName LIKE '%' + @1 + '%' OR u.DisplayName LIKE '%' + @1 + '%') ORDER BY u.DisplayName", portalId, search);
            }
        }
    }

    public partial interface ISpeakerRepository
    {
        IEnumerable<SpeakerWithNrSessions> GetSpeakersByConferenceWithNrSessions(int conferenceId, int sessionStatusThreshold);
        IEnumerable<DnnUser> SearchUsers(int portalId, string search);
    }
}

