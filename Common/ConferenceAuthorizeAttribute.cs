﻿using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;

namespace Connect.DNN.Modules.Conference.Common
{
    public enum SecurityAccessLevel
    {
        Anonymous = 0,
        Authenticated = 1,
        View = 2,
        Edit = 3,
        Admin = 4,
        Host = 5,
        SessionSubmit = 6,
        AttendConference = 7,
        ManageConference = 8
    }

    public class ConferenceAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        public SecurityAccessLevel SecurityLevel { get; set; }
        public UserInfo User { get; set; }
        public bool AllowApiKeyAccess { get; set; } = false;

        public ConferenceAuthorizeAttribute()
        {
            SecurityLevel = SecurityAccessLevel.Admin;
        }

        public ConferenceAuthorizeAttribute(SecurityAccessLevel accessLevel)
        {
            SecurityLevel = accessLevel;
        }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            if (SecurityLevel == SecurityAccessLevel.Anonymous)
            {
                return true;
            }
            User = HttpContextSource.Current.Request.IsAuthenticated ? UserController.Instance.GetCurrentUserInfo() : new UserInfo();
            if (AllowApiKeyAccess && User.UserID == -1 && HttpContextSource.Current.Request.Params["apikey"] != null)
            {
                var conferenceId = int.Parse(HttpContextSource.Current.Request.Params["conferenceid"]);
                var apiKey = Connect.Conference.Core.Repositories.ApiKeyRepository.Instance.GetApiKey(HttpContextSource.Current.Request.Params["apikey"]);
                if (apiKey != null && apiKey.Expires != null && apiKey.Expires < System.DateTime.Now)
                {
                    Connect.Conference.Core.Repositories.ApiKeyRepository.Instance.DeleteApiKey(apiKey.GetApiKeyBase());
                    apiKey = null;
                }
                if (apiKey != null && apiKey.ConferenceId == conferenceId)
                {
                    User = UserController.Instance.GetUserById(PortalSettings.Current.PortalId, apiKey.CreatedByUserID);
                    HttpContextSource.Current.Items["UserInfo"] = User; // Set thread user - this will expire after the request!
                }
            }
            ContextSecurity security = new ContextSecurity(context.ActionContext.Request.FindModuleInfo());
            switch (SecurityLevel)
            {
                case SecurityAccessLevel.Authenticated:
                    return User.UserID != -1;
                case SecurityAccessLevel.Host:
                    return User.IsSuperUser;
                case SecurityAccessLevel.Admin:
                    return security.IsAdmin;
                case SecurityAccessLevel.Edit:
                    return security.CanEdit;
                case SecurityAccessLevel.View:
                    return security.CanView;
                case SecurityAccessLevel.SessionSubmit:
                    return security.CanSubmitSessions;
                case SecurityAccessLevel.AttendConference:
                    return security.CanAttend;
                case SecurityAccessLevel.ManageConference:
                    return security.CanManage;
            }
            return false;
        }
    }
}