using System;
using System.Collections.Generic;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using Connect.Conference.Core.Models.Speakers;

namespace Connect.Conference.Core.Repositories
{

    public partial class SpeakerRepository : ServiceLocator<ISpeakerRepository, SpeakerRepository>, ISpeakerRepository
    {
        protected override Func<ISpeakerRepository> GetFactory()
        {
            return () => new SpeakerRepository();
        }
        //public IEnumerable<Speaker> GetSpeakersByConference(int conferenceId)
        //{
        //    using (var context = DataContext.Instance())
        //    {
        //        return context.ExecuteQuery<Speaker>(System.Data.CommandType.Text,
        //            "SELECT * FROM {databaseOwner}{objectQualifier}vw_Connect_Conference_Speakers WHERE ConferenceId=@0",
        //            conferenceId);
        //    }
        //}
        public IEnumerable<Speaker> GetSpeakersByUser(int userId)
        {
            using (var context = DataContext.Instance())
            {
                return context.ExecuteQuery<Speaker>(System.Data.CommandType.Text,
                    "SELECT * FROM {databaseOwner}{objectQualifier}vw_Connect_Conference_Speakers WHERE UserId=@0",
                    userId);
            }
        }
        public Speaker GetSpeaker(int conferenceId, int userId)
        {
            using (var context = DataContext.Instance())
            {
                return context.ExecuteSingleOrDefault<Speaker>(System.Data.CommandType.Text,
                    "SELECT * FROM {databaseOwner}{objectQualifier}vw_Connect_Conference_Speakers WHERE ConferenceId=@0 AND UserId=@1",
                    conferenceId, userId);
            }
        }
        public void AddSpeaker(SpeakerBase speaker, int userId)
        {
            Requires.NotNull(speaker);
            Requires.NotNull(speaker.ConferenceId);
            Requires.NotNull(speaker.UserId);
            speaker.CreatedByUserID = userId;
            speaker.CreatedOnDate = DateTime.Now;
            speaker.LastModifiedByUserID = userId;
            speaker.LastModifiedOnDate = DateTime.Now;
            using (var context = DataContext.Instance())
            {
                context.Execute(System.Data.CommandType.Text,
                    "IF NOT EXISTS (SELECT * FROM {databaseOwner}{objectQualifier}Connect_Conference_Speakers " +
                    "WHERE ConferenceId=@0 AND UserId=@1) " +
                    "INSERT INTO {databaseOwner}{objectQualifier}Connect_Conference_Speakers (ConferenceId, UserId, Company, Sort, CreatedByUserID, CreatedOnDate, LastModifiedByUserID, LastModifiedOnDate, Url, Description, DescriptionShort) " +
                    "SELECT @0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10", speaker.ConferenceId, speaker.UserId, speaker.Company, speaker.Sort, speaker.CreatedByUserID, speaker.CreatedOnDate, speaker.LastModifiedByUserID, speaker.LastModifiedOnDate, speaker.Url, speaker.Description, speaker.DescriptionShort);
            }
        }
        public void DeleteSpeaker(SpeakerBase speaker)
        {
            DeleteSpeaker(speaker.ConferenceId, speaker.UserId);
        }
        public void DeleteSpeaker(int conferenceId, int userId)
        {
            Requires.NotNull(conferenceId);
            Requires.NotNull(userId);
            using (var context = DataContext.Instance())
            {
                context.Execute(System.Data.CommandType.Text,
                    "DELETE FROM {databaseOwner}{objectQualifier}Connect_Conference_Speakers WHERE ConferenceId=@0 AND UserId=@1",
                    conferenceId, userId);
            }
        }
        public void DeleteSpeakersByConference(int conferenceId)
        {
            using (var context = DataContext.Instance())
            {
                context.Execute(System.Data.CommandType.Text,
                    "DELETE FROM {databaseOwner}{objectQualifier}Connect_Conference_Speakers WHERE ConferenceId=@0",
                    conferenceId);
            }
        }
        public void DeleteSpeakersByUser(int userId)
        {
            using (var context = DataContext.Instance())
            {
                context.Execute(System.Data.CommandType.Text,
                    "DELETE FROM {databaseOwner}{objectQualifier}Connect_Conference_Speakers WHERE UserId=@0",
                    userId);
            }
        }
        public void UpdateSpeaker(SpeakerBase speaker, int userId)
        {
            Requires.NotNull(speaker);
            Requires.NotNull(speaker.ConferenceId);
            Requires.NotNull(speaker.UserId);
            speaker.LastModifiedByUserID = userId;
            speaker.LastModifiedOnDate = DateTime.Now;
            using (var context = DataContext.Instance())
            {
                var rep = context.GetRepository<SpeakerBase>();
                rep.Update("SET Company=@0, Sort=@1, CreatedByUserID=@2, CreatedOnDate=@3, LastModifiedByUserID=@4, LastModifiedOnDate=@5, Url=@6, Description=@7, DescriptionShort=@8 WHERE ConferenceId=@9 AND UserId=@10",
                          speaker.Company, speaker.Sort, speaker.CreatedByUserID, speaker.CreatedOnDate, speaker.LastModifiedByUserID, speaker.LastModifiedOnDate, speaker.Url, speaker.Description, speaker.DescriptionShort, speaker.ConferenceId, speaker.UserId);
            }
        }
    }

    public partial interface ISpeakerRepository
    {
        IEnumerable<SpeakerWithNrSessions> GetSpeakersByConference(int conferenceId);
        IEnumerable<Speaker> GetSpeakersByUser(int userId);
        Speaker GetSpeaker(int conferenceId, int userId);
        void AddSpeaker(SpeakerBase speaker, int userId);
        void DeleteSpeaker(SpeakerBase speaker);
        void DeleteSpeaker(int conferenceId, int userId);
        void DeleteSpeakersByConference(int conferenceId);
        void DeleteSpeakersByUser(int userId);
        void UpdateSpeaker(SpeakerBase speaker, int userId);
    }
}

