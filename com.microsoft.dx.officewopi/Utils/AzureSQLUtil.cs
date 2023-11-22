using com.chalkline.wopi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading.Tasks;

namespace com.chalkline.wopi.Utils
{
    public class AzureSQLUtil
    {
        public static async Task<byte[]> GetBlob(string guid, string ownerId)
        {
            byte[] bytes = null;
            try
            {
                System.Diagnostics.Trace.WriteLine("file id:");
                System.Diagnostics.Trace.WriteLine(guid);

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = ConfigurationManager.AppSettings["sql:DataSource"],
                    UserID = ConfigurationManager.AppSettings["sql:UserID"],
                    Password = ConfigurationManager.AppSettings["sql:Password"],
                    InitialCatalog = ConfigurationManager.AppSettings["sql:InitialCatalog"]
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    System.Diagnostics.Trace.WriteLine("Query data example:");
                    System.Diagnostics.Trace.WriteLine("=========================================\n");

                    String sql = "SELECT fileName, convert(nvarchar(50), guid) AS guid, blob FROM dbo.files WHERE guid = @guid AND OwnerID = @ownerId";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@guid", System.Data.SqlDbType.NVarChar).Value = guid;
                        command.Parameters.Add("@ownerId", System.Data.SqlDbType.NVarChar).Value = ownerId;
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                System.Diagnostics.Trace.WriteLine(reader.GetString(0));
                                System.Diagnostics.Trace.WriteLine(reader.GetString(1));
                                bytes = (byte[])reader["blob"];
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
                return null;
            }

            return bytes;
        }

        public static async Task UploadFile(string guid, byte[] fileBytes, string fileName, string ownerId, string version)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("file id:");
                System.Diagnostics.Trace.WriteLine(guid);

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = ConfigurationManager.AppSettings["sql:DataSource"],
                    UserID = ConfigurationManager.AppSettings["sql:UserID"],
                    Password = ConfigurationManager.AppSettings["sql:Password"],
                    InitialCatalog = ConfigurationManager.AppSettings["sql:InitialCatalog"]
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("INSERT INTO dbo.Files (GUID,Blob,FileName,OwnerId,Version) Values (@guid,@blob,@fileName,@ownerId,@version)", connection))
                    {
                        command.Parameters.Add("@guid", System.Data.SqlDbType.NVarChar).Value = guid;
                        command.Parameters.Add("@blob", SqlDbType.VarBinary, fileBytes.Length).Value = fileBytes;
                        command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar).Value = fileName;
                        command.Parameters.Add("@ownerId", System.Data.SqlDbType.NVarChar).Value = ownerId;
                        command.Parameters.Add("@version", System.Data.SqlDbType.NVarChar).Value = version;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }

        public static async Task<bool> DeleteFile(string guid)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("file id:");
                System.Diagnostics.Trace.WriteLine(guid);

                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = ConfigurationManager.AppSettings["sql:DataSource"],
                    UserID = ConfigurationManager.AppSettings["sql:UserID"],
                    Password = ConfigurationManager.AppSettings["sql:Password"],
                    InitialCatalog = ConfigurationManager.AppSettings["sql:InitialCatalog"]
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("DELETE dbo.Files WHERE GUID = @guid", connection))
                    {
                        command.Parameters.Add("@guid", System.Data.SqlDbType.NVarChar).Value = guid;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
                return false;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
                return false;
            }
            return true;
        }

        public static IEnumerable<DetailedFileModel> GetItems(string ownerId)
        {
            List<DetailedFileModel> files = new List<DetailedFileModel>();

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = ConfigurationManager.AppSettings["sql:DataSource"],
                    UserID = ConfigurationManager.AppSettings["sql:UserID"],
                    Password = ConfigurationManager.AppSettings["sql:Password"],
                    InitialCatalog = ConfigurationManager.AppSettings["sql:InitialCatalog"]
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    string sql = "SELECT GUID, OwnerId, FileName, Version, LockValue, LockExpires,";
                    sql += " DATALENGTH(Blob) AS Bytes FROM dbo.Files WHERE OwnerId = @OwnerId Order by FileName ASC";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@OwnerId", System.Data.SqlDbType.NVarChar).Value = ownerId;
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Create the file entity
                                DetailedFileModel file = new DetailedFileModel()
                                {
                                    id = (Guid)reader.GetSqlGuid(0),
                                    OwnerId = reader.GetString(1),
                                    BaseFileName = reader.GetString(2),
                                    Version = reader.GetSqlInt32(3).ToString(),
                                    LockValue = reader.IsDBNull(4) ? null : reader.GetString(4).Trim(),
                                    LockExpires = reader.IsDBNull(5) ? new DateTime().AddYears(10) : reader.GetDateTime(5),
                                    Size = (long)reader.GetSqlInt64(6)
                                };
                                files.Add(file);
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
                return null;
            }

            return files;

        }

        public static DetailedFileModel GetItem(string guid)
        {
            DetailedFileModel file = new DetailedFileModel();

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = ConfigurationManager.AppSettings["sql:DataSource"],
                    UserID = ConfigurationManager.AppSettings["sql:UserID"],
                    Password = ConfigurationManager.AppSettings["sql:Password"],
                    InitialCatalog = ConfigurationManager.AppSettings["sql:InitialCatalog"]
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    String sql = "SELECT guid, OwnerId, fileName, Version, LockValue, LockExpires, DATALENGTH(blob) AS Bytes FROM dbo.files WHERE guid = @guid";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@guid", System.Data.SqlDbType.NVarChar).Value = guid;
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Create the file entity
                                file = new DetailedFileModel()
                                {
                                    id = (Guid)reader.GetSqlGuid(0),
                                    OwnerId = reader.GetString(1),
                                    BaseFileName = reader.GetString(2),
                                    Version = reader.GetSqlInt32(3).ToString(),
                                    LockValue = reader.IsDBNull(4) ? null : reader.GetString(4).Trim(),
                                    LockExpires = reader.IsDBNull(5) ? new DateTime().AddYears(10) : reader.GetDateTime(5),
                                    Size = (long)reader.GetSqlInt64(6)
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
                return null;
            }
            return file;
        }

        public static async Task UpdateItem(FileModel file)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = ConfigurationManager.AppSettings["sql:DataSource"],
                    UserID = ConfigurationManager.AppSettings["sql:UserID"],
                    Password = ConfigurationManager.AppSettings["sql:Password"],
                    InitialCatalog = ConfigurationManager.AppSettings["sql:InitialCatalog"]
                };

                using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
                {
                    connection.Open();
                    string commandBuilder = "UPDATE dbo.Files SET";
                    commandBuilder += " UpdatedOn = GETDATE(),";
                    commandBuilder += " OwnerId = @ownerId,";
                    commandBuilder += " FileName = @fileName,";
                    commandBuilder += " Version = @version,";
                    commandBuilder += " LockValue = @lockValue,";
                    commandBuilder += " LockExpires = @lockExpires";
                    commandBuilder += " WHERE GUID = @guid";
                    commandBuilder += " AND Version = @version";
                    using (var command = new SqlCommand(commandBuilder, connection))
                    {
                        command.Parameters.Add("@ownerId", System.Data.SqlDbType.NVarChar).Value = file.OwnerId;
                        command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar).Value = file.BaseFileName;
                        command.Parameters.Add("@version", System.Data.SqlDbType.NVarChar).Value = file.Version;
                        command.Parameters.Add("@lockValue", System.Data.SqlDbType.NVarChar).Value = file.LockValue ?? SqlString.Null;
                        command.Parameters.Add("@lockExpires", System.Data.SqlDbType.DateTime).Value = file.LockExpires ?? SqlDateTime.Null;
                        ;
                        command.Parameters.Add("@guid", System.Data.SqlDbType.NVarChar).Value = file.id.ToString();
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }
        }
    }
}