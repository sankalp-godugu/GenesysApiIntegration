namespace GenesysContactsProcessJob.Utilities
{
    public class ConfigConstants
    {
        public static string TokenUrlKey => "Genesys:AppConfigurations:AccessTokenUrl";
        public static string GrantTypeKey => "Genesys:AppConfigurations:GrantType";
        public static string ClientIdKey => "Genesys:AppConfigurations:ClientId";
        public static string ClientSecretKey => "Genesys:AppConfigurations:ClientSecret";
        public static string BaseUrlKey => "Genesys:AppConfigurations:BaseUrl";
        public static string ContactListIdAetnaEnglishKey => "Genesys:AppConfigurations:AetnaEnglish";
        public static string ContactListIdAetnaSpanishKey => "Genesys:AppConfigurations:AetnaSpanish";
        public static string GetContacts => "Genesys:ApiEndPoints:GetContacts";
        public static string RemoveContacts => "Genesys:ApiEndPoints:RemoveContacts";
        public static string AddContacts => "Genesys:ApiEndPoints:AddContacts";
        public static string UpdateNoDialContacts => "Genesys:ApiEndPoints:UpdateContact";
        public static string UpdateAndDialContacts => "Genesys:ApiEndPoints:UpdateAndDialContacts";
    }
}
