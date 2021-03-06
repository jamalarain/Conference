
using System;
using System.Runtime.Serialization;
using DotNetNuke.ComponentModel.DataAnnotations;
using Connect.Conference.Core.Data;
using Connect.Conference.Core.Common;

namespace Connect.Conference.Core.Models.Tags
{
    [TableName("Connect_Conference_Tags")]
    [PrimaryKey("TagId", AutoIncrement = true)]
    [DataContract]
    [Scope("ConferenceId")]
    public partial class TagBase  : AuditableEntity 
    {

        #region .ctor
        public TagBase()
        {
            TagId = -1;
        }
        #endregion

        #region Properties
        [DataMember]
        public int TagId { get; set; }
        [DataMember]
        [WebApiSecurity(WebApiSecurityLevel.Management)]
        public int ConferenceId { get; set; }
        [DataMember]
        public string TagName { get; set; }
        #endregion

        #region Methods
        public void ReadTagBase(TagBase tag)
        {
            if (tag.TagId > -1)
                TagId = tag.TagId;

            if (tag.ConferenceId > -1)
                ConferenceId = tag.ConferenceId;

            if (!String.IsNullOrEmpty(tag.TagName))
                TagName = tag.TagName;

        }
        #endregion

    }
}



