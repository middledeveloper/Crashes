using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Hosting;
using Crashes.Models;

namespace Crashes.Controllers
{
    public class DB
    {
        public static Role GetRole(string account)
        {
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    var workerGilds = new List<int>();
                    using (var cmd = new SqlCommand("SELECT Workers.*, Roles.name FROM Workers " +
                                "JOIN Roles ON Workers.roleId = Roles.id WHERE account = @account ", con))
                    {
                        cmd.Parameters.AddWithValue("@account", account);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                var role = new Role()
                                {
                                    WorkerId = (int)dr["id"],
                                    WorkerName = AD.GetName(account),
                                    Account = account,
                                    RoleId = (int)dr["roleId"],
                                    RoleName = (string)dr["name"],
                                    GildIds = ((string)dr["gildIds"]).Split(',').Select(int.Parse).ToList()
                                };

                                if (role.RoleId == 1)
                                {
                                    role.GildIds = DB.GetGildRepo().Select(x => x.Id).ToList();
                                }

                                role.ActiveGild = role.GildIds.First();
                                return role;
                            }
                            else
                            {
                                var role = new Role()
                                {
                                    WorkerName = AD.GetName(account),
                                    Account = account,
                                    RoleId = 3,
                                    RoleName = "Читатель",
                                    //RoleId = 0,
                                    //RoleName = "Гость",
                                    GildIds = DB.GetGildRepo().Select(x => x.Id).ToList()
                                };

                                role.ActiveGild = role.GildIds.First();
                                return role;
                            }
                        }
                    }
                }
            }
        }

        public static Role GetRole(int workerId)
        {
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    var workerGilds = new List<int>();
                    using (var cmd = new SqlCommand("SELECT Workers.*, Roles.name FROM Workers " +
                                "JOIN Roles ON Workers.roleId = Roles.id WHERE Workers.id = @workerId ", con))
                    {
                        cmd.Parameters.AddWithValue("@workerId", workerId);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                try
                                {
                                    var role = new Role()
                                    {
                                        WorkerId = (int)dr["id"],
                                        WorkerName = AD.GetName((string)dr["account"]),
                                        Account = (string)dr["account"],
                                        RoleId = (int)dr["roleId"],
                                        RoleName = (string)dr["name"],
                                        GildIds = ((string)dr["gildIds"]).Split(',').Select(int.Parse).ToList()
                                    };

                                    if (role.RoleId == 1)
                                    {
                                        role.GildIds = DB.GetGildRepo().Select(x => x.Id).ToList();
                                    }

                                    role.ActiveGild = role.GildIds.First();
                                    return role;
                                }
                                catch (NullReferenceException)
                                {
                                    return null;
                                }
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static Gild GetGild(int gildId)
        {
            var gilds = new List<Gild>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    var query = "SELECT * FROM Gilds WHERE id = @gildId ";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@gildId", gildId);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                return new Gild()
                                {
                                    Id = (int)dr["id"],
                                    FoundryId = (int)dr["foundryId"],
                                    Name = (string)dr["name"],
                                    ReportOrder = (int)dr["reportOrder"]
                                };
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static int GetGildIdByEquipmentId(int gildId)
        {
            var gilds = new List<Gild>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    var query = "SELECT Equipment.id, Sectors.gildId FROM Equipment " +
                                "JOIN Sectors ON Equipment.sectorId = Sectors.id " +
                                "WHERE Equipment.id = @gildId ";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@gildId", gildId);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                                return (int)dr["gildId"];
                            else
                                return 0;
                        }
                    }
                }
            }
        }

        public static List<Foundry> GetFoundryRepo()
        {
            var foundries = new List<Foundry>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    var query = "SELECT * FROM Foundries ";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    foundries.Add(new Foundry()
                                    {
                                        Id = (int)dr["id"],
                                        Name = (string)dr["name"],
                                        ReportOrder = (int)dr["reportOrder"],
                                    });
                                }

                                return foundries;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static List<Gild> GetGildRepo(List<int> gildIds = null)
        {
            var gilds = new List<Gild>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("SELECT * FROM Gilds ", con))
                    {
                        if (gildIds != null && gildIds.First() != 0)
                        {
                            cmd.CommandText += " WHERE id = @gildId ";

                            foreach (var gildId in gildIds)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@gildId", gildId);
                                using (var dr = cmd.ExecuteReader())
                                {
                                    if (dr.Read())
                                    {
                                        gilds.Add(new Gild()
                                        {
                                            Id = (int)dr["id"],
                                            FoundryId = (int)dr["foundryId"],
                                            Name = (string)dr["name"],
                                            ReportOrder = (int)dr["reportOrder"]
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (var dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    gilds.Add(new Gild()
                                    {
                                        Id = (int)dr["id"],
                                        FoundryId = (int)dr["foundryId"],
                                        Name = (string)dr["name"],
                                        ReportOrder = (int)dr["reportOrder"]
                                    });
                                }
                            }
                        }

                        return gilds;
                    }
                }
            }
        }


        public static List<Sector> GetSectorRepo(int? gildId = null)
        {
            var sectors = new List<Sector>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("SELECT * FROM Sectors ", con))
                    {
                        if (gildId != null && gildId != 0)
                        {
                            cmd.CommandText += "WHERE gildId = @gildId ";
                            cmd.Parameters.AddWithValue("@gildId", gildId);
                        }

                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    sectors.Add(new Sector()
                                    {
                                        Id = (int)dr["id"],
                                        GildId = (int)dr["gildId"],
                                        Name = (string)dr["name"],
                                        ReportOrder = (int)dr["reportOrder"]
                                    });
                                }

                                return sectors;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static List<Spec> GetSpecRepo()
        {
            var specs = new List<Spec>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    var query = "SELECT * FROM Specs ";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    specs.Add(new Spec()
                                    {
                                        Id = (int)dr["id"],
                                        SectorId = (int)dr["sectorId"],
                                        Name = (string)dr["name"],
                                        ReportOrder = (int)dr["reportOrder"],
                                    });
                                }

                                return specs;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static List<Equipment> GetEquipmentRepo(List<Sector> sectors = null)
        {
            var equipment = new List<Equipment>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("SELECT * FROM Equipment ", con))
                    {
                        if (sectors != null)
                        {
                            cmd.CommandText += "WHERE sectorId = @sectorId ";
                            foreach (var sector in sectors)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@sectorId", sector.Id);
                                using (var dr = cmd.ExecuteReader())
                                {
                                    if (dr.HasRows)
                                    {
                                        while (dr.Read())
                                        {
                                            equipment.Add(new Equipment()
                                            {
                                                Id = (int)dr["id"],
                                                SectorId = (int)dr["sectorId"],
                                                SpecId = (int)dr["specId"],
                                                Name = (string)dr["name"],
                                                Fullname = (string)dr["fullname"],
                                                Active = (bool)dr["active"]
                                            });
                                        }
                                    }
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            using (var dr = cmd.ExecuteReader())
                            {
                                if (dr.HasRows)
                                {
                                    while (dr.Read())
                                    {
                                        equipment.Add(new Equipment()
                                        {
                                            Id = (int)dr["id"],
                                            SectorId = (int)dr["sectorId"],
                                            SpecId = (int)dr["specId"],
                                            Name = (string)dr["name"],
                                            Fullname = (string)dr["fullname"],
                                            Active = (bool)dr["active"]
                                        });
                                    }
                                }
                            }
                        }

                        equipment = equipment.OrderBy(x => x.Name).ToList();
                        return equipment;
                    }
                }
            }
        }

        public static Crash GetCrashById(int id)
        {
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("SELECT Work.*, Equipment.sectorId, Sectors.gildId FROM Work " +
                                                    "JOIN Equipment ON Work.equipmentId = Equipment.id " +
                                                    "JOIN Sectors ON Equipment.sectorId = Sectors.id " +
                                                    "WHERE Work.id = @crashId", con))
                    {
                        cmd.Parameters.AddWithValue("@crashId", id);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                var crash = new Crash()
                                {
                                    Id = (int)dr["id"],
                                    GildId = (int)dr["gildId"],
                                    EquipmentId = (int)dr["equipmentId"],
                                    Role = new Role() { WorkerId = (int)dr["authorId"] },
                                    Reason = (string)dr["reason"],
                                    StatusId = (int)dr["statusId"],
                                    Start = (DateTime)dr["start"],
                                    Stop = (DateTime)dr["stop"]
                                };

                                if (crash.Start == crash.Stop) crash.Stop = null;
                                return crash;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static List<Crash> GetActiveCrashes(int gildId = 0)
        {
            var crashes = new List<Crash>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("SELECT Work.*, Equipment.sectorId, Sectors.gildId FROM Work " +
                                                    "JOIN Equipment ON Work.equipmentId = Equipment.id " +
                                                    "JOIN Sectors ON Equipment.sectorId = Sectors.id ", con))
                    {
                        if (gildId != 0) cmd.CommandText += "WHERE Work.statusId = 1 AND Sectors.gildId = @gildId " +
                                                            "ORDER BY Work.start DESC ";
                        else cmd.CommandText += "WHERE Work.statusId = 1 ORDER BY Work.start DESC ";

                        cmd.Parameters.AddWithValue("@gildId", gildId);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    crashes.Add(new Crash()
                                    {
                                        Id = (int)dr["id"],
                                        GildId = (int)dr["gildId"],
                                        EquipmentId = (int)dr["equipmentId"],
                                        Role = new Role() { WorkerId = (int)dr["authorId"] },
                                        Reason = (string)dr["reason"],
                                        StatusId = (int)dr["statusId"],
                                        Start = (DateTime)dr["start"],
                                        Stop = (DateTime)dr["stop"]
                                    });
                                }

                                return crashes;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static List<Crash> GetClosedCrashes(int gildId, int count)
        {
            var crashes = new List<Crash>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    var query = string.Format("SELECT TOP {0} Work.*, Equipment.sectorId, Sectors.gildId FROM Work " +
                                              "JOIN Equipment ON Work.equipmentId = Equipment.id " +
                                              "JOIN Sectors ON Equipment.sectorId = Sectors.id " +
                                              "WHERE Work.statusId = 2 AND Sectors.gildId = @gildId " +
                                              "ORDER BY Work.stop DESC ", count);
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@gildId", gildId);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    crashes.Add(new Crash()
                                    {
                                        Id = (int)dr["id"],
                                        GildId = (int)dr["gildId"],
                                        EquipmentId = (int)dr["equipmentId"],
                                        Role = new Role() { WorkerId = (int)dr["authorId"] },
                                        Reason = (string)dr["reason"],
                                        StatusId = (int)dr["statusId"],
                                        Start = (DateTime)dr["start"],
                                        Stop = (DateTime)dr["stop"]
                                    });
                                }

                                return crashes;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static List<Crash> GetCrashes(int equipmentId, DateTime start, DateTime stop)
        {
            var crashes = new List<Crash>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("SELECT * FROM Work WHERE " +
                        "equipmentId = @equipmentId AND start >= @start AND stop <= @stop " +
                        "ORDER BY stop DESC", con))
                    {
                        cmd.Parameters.AddWithValue("@equipmentId", equipmentId);
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@stop", stop);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    crashes.Add(new Crash()
                                    {
                                        Id = (int)dr["id"],
                                        GildId = (int)dr["gildId"],
                                        EquipmentId = (int)dr["equipmentId"],
                                        Role = new Role() { WorkerId = (int)dr["authorId"] },
                                        Reason = (string)dr["reason"],
                                        StatusId = (int)dr["statusId"],
                                        Start = (DateTime)dr["start"],
                                        Stop = (DateTime)dr["stop"]
                                    });
                                }

                                return crashes;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }

        public static void CreateCrash(Crash crash)
        {
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("INSERT INTO Work (equipmentId,authorId,reason,statusId,start,stop) " +
                                                    "VALUES (@equipmentId,@authorId,@reason,@statusId,@start,@stop) ", con))
                    {
                        cmd.Parameters.AddWithValue("@equipmentId", crash.EquipmentId);
                        cmd.Parameters.AddWithValue("@authorId", crash.Role.WorkerId);
                        cmd.Parameters.AddWithValue("@reason", crash.Reason);
                        cmd.Parameters.AddWithValue("@statusId", crash.Stop != null ? 2 : 1);
                        cmd.Parameters.AddWithValue("@start", crash.Start);
                        cmd.Parameters.AddWithValue("@stop", crash.Stop == null ? crash.Start : crash.Stop);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void UpdateCrash(Crash crash)
        {
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("UPDATE Work SET start = @start, stop = @stop, reason = @reason, " +
                                                    "statusId = @statusId WHERE id = @rowId ", con))
                    {
                        cmd.Parameters.AddWithValue("@start", crash.Start);
                        cmd.Parameters.AddWithValue("@stop", crash.Stop == null ? crash.Start : crash.Stop);
                        cmd.Parameters.AddWithValue("@reason", crash.Reason);
                        cmd.Parameters.AddWithValue("@statusId", crash.Stop != null ? 2 : 1);
                        cmd.Parameters.AddWithValue("@rowId", crash.Id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void MarkCrashDeleted(int rowId)
        {
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("UPDATE Work SET statusId = 3 WHERE id = @rowId ", con))
                    {
                        cmd.Parameters.AddWithValue("@rowId", rowId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static List<Crash> GetReportCrashes(DateTime start, DateTime stop)
        {
            var crashes = new List<Crash>();
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    var query = "SELECT Work.id, Work.start, Work.stop, Work.equipmentId, Work.authorId, Work.statusId, " +
                                "Work.reason, Equipment.sectorId, Sectors.gildId, Foundries.id AS foundryId, " +
                                "Sectors.reportOrder " +
                                "FROM Work " +
                                "JOIN Equipment ON Work.equipmentId = Equipment.id " +
                                "JOIN Sectors ON Equipment.sectorId = Sectors.id " +
                                "JOIN Gilds ON Sectors.gildId = Gilds.id " +
                                "JOIN Foundries ON Gilds.foundryId = Foundries.id " +
                                "WHERE Work.start >= @start AND Work.stop <= @stop AND Work.statusId <> 3 " +
                                "ORDER BY start ";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@stop", stop);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                while (dr.Read())
                                {
                                    crashes.Add(new Crash()
                                    {
                                        Id = (int)dr["id"],
                                        AuthorId = (int)dr["authorId"],
                                        Start = (DateTime)dr["start"],
                                        Stop = (DateTime)dr["stop"],
                                        EquipmentId = (int)dr["equipmentId"],
                                        StatusId = (int)dr["statusId"],
                                        Reason = (string)dr["reason"],
                                        Sector = (int)dr["sectorId"],
                                        GildId = (int)dr["gildId"],
                                        Foundry = (int)dr["foundryId"],
                                        Order = (int)dr["reportOrder"]
                                    });
                                }
                            }

                            return crashes;
                        }
                    }
                }
            }
        }

        public static Dictionary<int, string> GetStatusRepo()
        {
            using (HostingEnvironment.Impersonate())
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["SQL"].ConnectionString))
                {
                    if (con.State == ConnectionState.Closed) con.Open();
                    using (var cmd = new SqlCommand("SELECT * FROM Statuses ", con))
                    {
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.HasRows)
                            {
                                var statuses = new Dictionary<int, string>();
                                while (dr.Read())
                                {
                                    statuses.Add((int)dr["id"], (string)dr["name"]);
                                }

                                return statuses;
                            }
                            else
                                return null;
                        }
                    }
                }
            }
        }
    }
}