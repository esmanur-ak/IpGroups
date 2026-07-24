namespace IpGroups.Constants
{
    public static class Permissions
    {
        public static class Bina
        {
            public const string View = "Permissions.Bina.View";
            public const string Create = "Permissions.Bina.Create";
            public const string Edit = "Permissions.Bina.Edit";
            public const string Delete = "Permissions.Bina.Delete";
        }

        public static class Birim
        {
            public const string View = "Permissions.Birim.View";
            public const string Create = "Permissions.Birim.Create";
            public const string Edit = "Permissions.Birim.Edit";
            public const string Delete = "Permissions.Birim.Delete";
        }

        public static class Ip
        {
            public const string View = "Permissions.Ip.View";
            public const string Create = "Permissions.Ip.Create";
            public const string Edit = "Permissions.Ip.Edit";
            public const string Delete = "Permissions.Ip.Delete";
        }

        public static class Person
        {
            public const string View = "Permissions.Person.View";
            public const string Create = "Permissions.Person.Create";
            public const string Edit = "Permissions.Person.Edit";
            public const string Delete = "Permissions.Person.Delete";
        }

        public static class Role
        {
            public const string Manage = "Permissions.Role.Manage";
        }

        public static List<string> GetAllPermissions()
        {
            return new List<string>
            {
                Bina.View, Bina.Create, Bina.Edit, Bina.Delete,
                Birim.View, Birim.Create, Birim.Edit, Birim.Delete,
                Ip.View, Ip.Create, Ip.Edit, Ip.Delete,
                Person.View, Person.Create, Person.Edit, Person.Delete,
                Role.Manage
            };
        }
    }
}
