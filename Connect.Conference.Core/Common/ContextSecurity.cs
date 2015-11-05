using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

namespace Connect.Conference.Core.Common
{
    public class ContextSecurity
    {
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSpeaker { get; set; }
        public bool IsAttendee { get; set; }
        public bool CanSubmitSessions { get; set; }
        public bool CanApproveSessions { get; set; }
        public bool CanAttend { get; set; }
        public bool CanManageAttendees { get; set; }

        #region ctor
        public ContextSecurity(ModuleInfo objModule)
        {
            CanView = ModulePermissionController.CanViewModule(objModule);
            CanEdit = ModulePermissionController.HasModulePermission(objModule.ModulePermissions, "EDIT");
            CanSubmitSessions = ModulePermissionController.HasModulePermission(objModule.ModulePermissions, "SESSIONSUBMIT");
            CanApproveSessions = ModulePermissionController.HasModulePermission(objModule.ModulePermissions, "SESSIONAPPROVE");
            CanAttend = ModulePermissionController.HasModulePermission(objModule.ModulePermissions, "CANATTEND");
            CanManageAttendees = ModulePermissionController.HasModulePermission(objModule.ModulePermissions, "MANAGEATTENDEES");
            IsAdmin = PortalSecurity.IsInRole(PortalSettings.Current.AdministratorRoleName);
            // todo: speaker and attendee test
        }
        #endregion

    }
}