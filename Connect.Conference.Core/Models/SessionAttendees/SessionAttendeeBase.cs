
using System;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Connect.Conference.Core.Data;
using Connect.Conference.Core.Common;

namespace Connect.Conference.Core.Models.SessionAttendees
{
    [TableName("Connect_Conference_SessionAttendees")]
    [DataContract]
    public partial class SessionAttendeeBase  : AuditableEntity 
    {

        #region .ctor
        public SessionAttendeeBase()
        {
        }
        #endregion

        #region Properties
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Management)]
        public int SessionId { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Management)]
        public int UserId { get; set; }
        #endregion

        #region Methods
        public void ReadSessionAttendeeBase(SessionAttendeeBase sessionAttendee)
        {
            if (sessionAttendee.SessionId > -1)
                SessionId = sessionAttendee.SessionId;

            if (sessionAttendee.UserId > -1)
                UserId = sessionAttendee.UserId;

        }
        #endregion

    }
}



