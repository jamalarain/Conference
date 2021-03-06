
using Connect.Conference.Core.Common;
using Connect.Conference.Core.Data;
using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using System.Runtime.Serialization;

namespace Connect.Conference.Core.Models.Conferences
{
    [TableName("Connect_Conference_Conferences")]
    [PrimaryKey("ConferenceId", AutoIncrement = true)]
    [DataContract]
    [Scope("PortalId")]
    public partial class ConferenceBase : AuditableEntity
    {

        #region .ctor
        public ConferenceBase()
        {
            ConferenceId = -1;
            AttendeeRole = -1;
            SpeakerRole = -1;
        }
        #endregion

        #region Properties
        [DataMember]
        public int ConferenceId { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Management)]
        public int PortalId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public DateTime? StartDate { get; set; }
        [DataMember]
        public DateTime? EndDate { get; set; }
        [DataMember]
        public int? MaxCapacity { get; set; }
        [DataMember]
        public bool SessionVoting { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Private)]
        public int AttendeeRole { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Private)]
        public int SpeakerRole { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string Url { get; set; }
        [DataMember]
        public bool SubmittedSessionsPublic { get; set; }
        [DataMember]
        public string TimeZoneId { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Private)]
        public string MqttBroker { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Private)]
        public string MqttBrokerUsername { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Private)]
        public string MqttBrokerPassword { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Private)]
        public string BaseTopicPath { get; set; }
        #endregion

        #region Methods
        public void ReadConferenceBase(ConferenceBase conference)
        {
            if (conference.ConferenceId > -1)
            {
                ConferenceId = conference.ConferenceId;
            }

            if (conference.PortalId > -1)
            {
                PortalId = conference.PortalId;
            }

            if (!String.IsNullOrEmpty(conference.Name))
            {
                Name = conference.Name;
            }

            if (!String.IsNullOrEmpty(conference.Description))
            {
                Description = conference.Description;
            }

            if (conference.StartDate != null)
            {
                StartDate = conference.StartDate;
            }

            if (conference.EndDate != null)
            {
                EndDate = conference.EndDate;
            }

            if (conference.MaxCapacity > -1)
            {
                MaxCapacity = conference.MaxCapacity;
            }

            SessionVoting = conference.SessionVoting;

            if (conference.AttendeeRole > -1)
            {
                AttendeeRole = conference.AttendeeRole;
            }

            if (conference.SpeakerRole > -1)
            {
                SpeakerRole = conference.SpeakerRole;
            }

            if (!String.IsNullOrEmpty(conference.Location))
            {
                Location = conference.Location;
            }

            if (!String.IsNullOrEmpty(conference.Url))
            {
                Url = conference.Url;
            }

            SubmittedSessionsPublic = conference.SubmittedSessionsPublic;

            if (!String.IsNullOrEmpty(conference.TimeZoneId))
            {
                TimeZoneId = conference.TimeZoneId;
            }

            if (!String.IsNullOrEmpty(conference.MqttBroker))
            {
                MqttBroker = conference.MqttBroker;
            }

            if (!String.IsNullOrEmpty(conference.MqttBrokerUsername))
            {
                MqttBrokerUsername = conference.MqttBrokerUsername;
            }

            if (!String.IsNullOrEmpty(conference.MqttBrokerPassword))
            {
                MqttBrokerPassword = conference.MqttBrokerPassword;
            }

            if (!String.IsNullOrEmpty(conference.BaseTopicPath))
            {
                BaseTopicPath = conference.BaseTopicPath;
            }
        }
        #endregion

    }
}



