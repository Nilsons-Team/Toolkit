namespace Toolkit_Shared.Network.Packets
{
    public enum PacketType : int
    {
        NONE = 0,

        APP_PACKET = 1,
        APP_KEY_PACKET = 2,
        APP_PURCHASE_METHOD_PACKET = 3,
        APP_STORE_PAGE_PACKET = 4,
        APP_TYPE_PACKET = 5,
        CATEGORY_PACKET = 6,
        COMPANY_PACKET = 7,
        COUNTRY_PACKET = 8,
        TAG_PACKET = 9,
        USER_PACKET = 10,
        USER_PURCHASED_APP_PACKET = 11,

        USER_AUTH_PACKET = 12
    }

    public struct UserAuthPacket
    {
        public PacketType packetType;
        public string Login;
        public string Password;

        public override string ToString()
        {
            return $"Login='{Login}', Password='{Password}'";
        }
    }

    public struct AppPacket
    {
        public const int NAME_LENGTH = 128;
        public const int UPLOAD_DATETIME_LENGTH = 19;
        public const int RELEASE_DATETIME_LEGNTH = 19;
        public const int DISCOUNT_START_DATETIME_LENGTH = 19;
        public const int DISCOUNT_EXPIRE_DATETIME_LENGTH = 19;
        
        public PacketType packetType;
        public long   Id;
        public string Name;
        public long   DeveloperCompanyId;
        public long   PublisherCompanyId;
        public string UploadDatetime;
        public string ReleaseDatetime;
        public long   DiscountPercent;
        public string DiscountStartDatetime;
        public string DiscountExpireDatetime;
        public long   AppTypeId;
        public long   AppReleaseStateId;

        public override string ToString()
        {
            return $"Id='{Id}', Name='{Name}'";
        }
    }
}
