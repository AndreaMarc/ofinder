using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using MIT.Fwk.Core.Attributes;

namespace MIT.Fwk.Infrastructure.Entities
{
    [SkipJwtAuthentication(JwtHttpMethod.GET)]
    public class Setup : Identifiable<int>
    {
        [Attr]
        public string environment { get; set; }

        [Attr]
        public string minAppVersion { get; set; }

        [Attr]
        public bool maintenance { get; set; }

        [Attr]
        public bool useRemoteFiles { get; set; }

        [Attr]
        public bool disableLog { get; set; }

        [Attr]
        public bool publicRegistration { get; set; }

        [Attr]
        public string sliderPosition { get; set; }

        [Attr]
        public string sliderPics { get; set; }


        [Attr]
        public string availableLanguages { get; set; }

        [Attr]
        public string defaultLanguage { get; set; }

        [Attr]
        public int failedLoginAttempts { get; set; }

        [Attr]
        public int previousPasswordsStored { get; set; }

        [Attr]
        public string defaultUserPassword { get; set; }

        [Attr]
        public string languageSetup { get; set; }

        [Attr]
        public int passwordExpirationPeriod { get; set; }

        [Attr]
        public int blockingPeriodDuration { get; set; }

        [Attr]
        public string sliderRegistrationPosition { get; set; }

        [Attr]
        public string sliderTermsPosition { get; set; }

        [Attr]
        public string headerLight { get; set; }

        [Attr]
        public string sidebarLight { get; set; }

        [Attr]
        public string headerBackground { get; set; }

        [Attr]
        public string sidebarBackground { get; set; }

        [Attr]
        public string beLanguage { get; set; }

        [Attr]
        public bool fixedSidebar { get; set; }

        [Attr]
        public bool fixedFooter { get; set; }

        [Attr]
        public bool fixedHeader { get; set; }

        [Attr]
        public bool bodyTabsShadow { get; set; }

        [Attr]
        public bool bodyTabsLine { get; set; }

        [Attr]
        public bool appThemeWhite { get; set; }

        [Attr]
        public bool headerShadow { get; set; }

        [Attr]
        public bool sidebarShadow { get; set; }

        [Attr]
        public bool useUrlStaticFiles { get; set; }

        [Attr]
        public string entitiesList { get; set; }

        [Attr]
        public string routesList { get; set; }

        [Attr]
        public string defaultClaims { get; set; }
        [Attr]
        public int accessTokenExpiresIn { get; set; }

        [Attr]
        public int refreshTokenExpiresIn { get; set; }


        [Attr]
        public bool canChangeTenants { get; set; }

        [Attr]
        public string internalChat { get; set; }

        [Attr]
        public bool internalNotifications { get; set; }

        [Attr]
        public bool pushNotifications { get; set; }

        [Attr]
        public bool canSearch { get; set; }

        [Attr]
        public string registrationFields { get; set; }

        [Attr]
        public int mailTokenExpiresIn { get; set; }

        [Attr]
        public bool mailerUsesAltText { get; set; }

        [Attr]
        public bool forceLoginRedirect { get; set; }

        [Attr]
        public bool needRequestAssociation { get; set; }

        [Attr]
        public string maeUsers { get; set; }

        [Attr]
        public string thirdPartsAccesses { get; set; }

        [Attr]
        public string googleCredentials { get; set; }
        [Attr]
        public bool useMD5 { get; set; }
        [Attr]
        public bool logicDelete { get; set; }

        [Attr]
        public string rolesForEditUsers { get; set; }

        [Attr]
        public string applicationName { get; set; }



    }
}
