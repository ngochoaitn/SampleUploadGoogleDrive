using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UploadGoogleDriver
{
    /// <summary>
    /// Cần tạo ứng dụng trước tại https://console.cloud.google.com/appengine/start?hl=en
    /// </summary>
    public class GoogleDriveApi
    {
        static string crePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Templates), "GPM", "GoogleApi", "GoogleDrive");
        static FileDataStore _fileDataStore = new FileDataStore(crePath, true);
        private DriveService _service;
        
        public string LocalUserName { set; get; }
        string _clientId,_clientSecret, _appName="GPM.Apis.GoogleApi.Drive";

        public GoogleDriveApi(string clientid, string clientsecret, string localuser) : this(clientid, clientsecret)
        {
            this.LocalUserName = localuser;
        }

        public GoogleDriveApi(string clientid, string clientsecret)
        {
            this._clientId = clientid;
            this._clientSecret = clientsecret;
        }

        /// <summary>
        /// Kết nối với tài khoản Google Drive, nếu kết nối rồi thì sẽ chạy hàm xong luôn
        /// </summary>
        public async Task LoginAsync()
        {
            _fileDataStore = new FileDataStore(crePath, true);
            var app = new ClientSecrets() { ClientId = this._clientId, ClientSecret = this._clientSecret };
            var scopes = new[] { DriveService.Scope.Drive,
                                 DriveService.Scope.DriveFile,
                                 DriveService.Scope.DriveAppdata,
                                 DriveService.Scope.DriveReadonly};

            //Kiểm tra thông tin đã xác thực chưa? Nếu chưa thì yêu cầu xác thực
            UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                app,
                scopes,
                this.LocalUserName,
                System.Threading.CancellationToken.None,
                _fileDataStore);

            //Tạo service
            _service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _appName
            });
        }

        /// <summary>
        /// Kiểm tra trang thái kết nối với tài khoản Google. True nếu đã kết nối thành công
        /// </summary>
        /// <returns></returns>
        public bool CheckLogin()
        {
            return CheckLogin(this.LocalUserName);
        }

        /// <summary>
        /// Kiểm tra trang thái kết nối với tài khoản Google. True nếu đã kết nối thành công
        /// </summary>
        /// <returns></returns>
        public static bool CheckLogin(string localusername)
        {
            string fileSave = string.Format("{0}\\{1}", _fileDataStore.FolderPath,  "Google.Apis.Auth.OAuth2.Responses.TokenResponse-" + localusername);
            return System.IO.File.Exists(fileSave);
        }

        /// <summary>
        /// Ngắt kết nối với tài khoản Google
        /// </summary>
        /// <returns></returns>
        public void Signout()
        {
            Signout(this.LocalUserName);
        }

        /// <summary>
        /// Ngắt kết nối với tài khoản Google
        /// </summary>
        /// <returns></returns>
        public static void Signout(string localusername)
        {
            string fileSave = string.Format("{0}\\{1}", _fileDataStore.FolderPath,  "Google.Apis.Auth.OAuth2.Responses.TokenResponse-" + localusername);
            if (System.IO.File.Exists(fileSave))
                System.IO.File.Delete(fileSave);
        }

        private void PrintListOfFile()
        {
            var listRequest = _service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name, webContentLink, webViewLink)";
            var exe = listRequest.Execute();
            var files = exe.Files;

            foreach (var f in files)
                Console.WriteLine("{0} {1}", f.Name, f.Id, f.WebContentLink, f.WebViewLink);
        }

        /// <summary>
        /// Lấy danh sách tệp
        /// </summary>
        /// <returns></returns>
        public List<Google.Apis.Drive.v3.Data.File> GetListFile()
        {
            List<Google.Apis.Drive.v3.Data.File> res = new List<Google.Apis.Drive.v3.Data.File>();

            FilesResource.ListRequest listRequest;
            FileList fileList;
            listRequest = _service.Files.List();
            listRequest.Fields = "nextPageToken, files(id, name, webContentLink, webViewLink)";

            do
            {
                fileList = listRequest.Execute();

                var files = fileList.Files;
                foreach (var f in files)
                    res.Add(f);

                listRequest.PageToken = fileList.NextPageToken;

            } while (!string.IsNullOrEmpty(fileList.NextPageToken));

            return res;
        }

        /// <summary>
        /// Lấy danh sách tệp
        /// </summary>
        /// <returns></returns>
        /// <param name="cancel"></param>
        /// <param name="folderid">Folder Id cần lấy danh sách tệp, để null sẽ lấy tất cả</param>
        public async Task<List<Google.Apis.Drive.v3.Data.File>> GetListFileAsync(CancellationTokenSource cancel, string folderid = null)
        {
            List<Google.Apis.Drive.v3.Data.File> res = new List<Google.Apis.Drive.v3.Data.File>();

            FilesResource.ListRequest listRequest;
            FileList fileList;

            listRequest = _service.Files.List();

            listRequest.Q = folderid != null ? $"'{folderid}' in parents" : null;
            listRequest.Spaces = "drive";
            
            listRequest.Fields = "nextPageToken, files(id, name, webContentLink, webViewLink,spaces,parents)";
            do
            {
                fileList = await listRequest.ExecuteAsync(cancel.Token);

                var files = fileList.Files;
                foreach (var f in files)
                {
                    res.Add(f);
                }

                listRequest.PageToken = fileList.NextPageToken;

            } while (!string.IsNullOrEmpty(fileList.NextPageToken));

            return res;
        }

        /// <summary>
        /// Tải một tệp lên Drive
        /// </summary>
        /// <param name="filepath">Đường dẫn tệp trên máy</param>
        /// <param name="callback_progressuploadchanged">Thông báo tiến trình upload</param>
        /// <param name="mimetype"></param>
        /// <param name="shareanyonewithlink">Chia sẻ link sau khi tải lên xong</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string UploadFile(string filepath,
                                  Action<IUploadProgress> callback_progressuploadchanged = null,
                                  string mimetype = "video/audio/image/jpeg",
                                  bool shareanyonewithlink = false)
        {
            //Code vi du: https://developers.google.com/drive/v3/web/manage-uploads

            FileInfo fInfo = new FileInfo(filepath);
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fInfo.Name,
                Description = "Upload by tool"
            };

            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                var requestUpload = _service.Files.Create(fileMetadata, fs, mimetype);

                if (callback_progressuploadchanged != null)
                    requestUpload.ProgressChanged += callback_progressuploadchanged;
                requestUpload.Upload();

                if (requestUpload.ResponseBody == null)
                    throw new Exception("Upload fail");
                else
                {
                    if (shareanyonewithlink)
                    {
                        string linkShare = ShareAnyoneWithLink(requestUpload.ResponseBody.Id);
                        return linkShare;
                    }
                    else
                    {
                        return "Upload complete not share link";
                    }
                }
            }
        }

        /// <summary>
        /// Chia sẻ một file với tất cả mọi người
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        private string ShareAnyoneWithLink(string fileid)
        {
            Permission permision = new Permission()
            {
                Type="anyone",
                Role="reader"
            };
            var requestShare = _service.Permissions.Create(permision, fileid);
            requestShare.Execute();
            return string.Format("https://drive.google.com/file/d/{0}/view?usp=sharing", fileid);
        }

        /// <summary>
        /// Tải một tệp lên Drive
        /// </summary>
        /// <param name="filepath">Đường dẫn tệp trên máy</param>
        /// <param name="cancel_token"></param>
        /// <param name="folderid">Folder ID trên drive, null là upload lên thư mục gốc</param>
        /// <param name="callback_progressuploadchanged">Thông báo tiến trình upload</param>
        /// <param name="filename">Tên tệp trên drive, null là cùng với tên tệp trên máy</param>
        /// <param name="mimetype"></param>
        /// <param name="shareanyonewithlink">Chia sẻ link sau khi tải lên xong</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> UploadFileAsync(string filepath,
                                  CancellationToken cancel_token,
                                  string folderid = null,
                                  Action<double> callback_progressuploadchanged = null,
                                  string filename=null,
                                  string mimetype = "video/audio/image/jpeg",
                                  bool shareanyonewithlink = false)
        {
            //Code vi du: https://developers.google.com/drive/v3/web/manage-uploads

            FileInfo fInfo = new FileInfo(filepath);
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = filename == null ? fInfo.Name : filename,
                Description = "Upload by tool DRADYO",
                Parents = folderid != null ? new List<string> {folderid } : null
            };

            using (FileStream fs = new FileStream(filepath, FileMode.Open))
            {
                var requestUpload = _service.Files.Create(fileMetadata, fs, mimetype);

                if (callback_progressuploadchanged != null)
                    requestUpload.ProgressChanged += (u) => callback_progressuploadchanged((double)u.BytesSent / fs.Length);

                var upload_response = await requestUpload.UploadAsync(cancel_token);

                if (upload_response.Exception != null)
                {
                    //if(upload_response.Exception is Google.GoogleApiException)
                    
                    throw upload_response.Exception;
                }

                if (requestUpload.ResponseBody == null)
                    throw new Exception("Upload fail");
                else
                {
                    if (shareanyonewithlink)
                    {
                        string linkShare = await ShareAnyoneWithLinkAsync(_service, requestUpload.ResponseBody.Id);
                        //return linkShare;
                    }
                    
                    return requestUpload.ResponseBody.Id;
                }
            }
        }

        /// <summary>
        /// Chia sẻ một file với tất cả mọi người
        /// </summary>
        /// <param name="fileid"></param>
        /// <returns></returns>
        private async Task<string> ShareAnyoneWithLinkAsync(DriveService service, string fileid)
        {
            Permission permision = new Permission()
            {
                Type="anyone",
                Role="reader"
            };
            var requestShare = service.Permissions.Create(permision, fileid);
            await requestShare.ExecuteAsync();
            return $"https://drive.google.com/file/d/{fileid}/view?usp=sharing";
        }

        /// <summary>
        /// Lấy dung lượng còn trống
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetFreeSpaceAsync()
        {
            string jsonData = await _service.HttpClient.GetStringAsync("https://www.googleapis.com/drive/v2/about");
            var x = JObject.Parse(jsonData);
            long total = x["quotaBytesTotal"].ToObject<long>();
            long used = 0;

            foreach (var data in x["quotaBytesByService"])
                if (data["bytesUsed"] != null)
                    used += data["bytesUsed"].ToObject<long>();

            return total - used;
        }

        /// <summary>
        /// Tải một tệp về máy
        /// </summary>
        /// <param name="fileid">ID tệp trên drive</param>
        /// <param name="folderSaveOnPC">Thư mục sẽ tải tệp về. Tên tệp theo tên trên Drive, nếu cần tùy chỉnh tên tệp thì dùng hàm DownloadFileWithCustomFileNameAsync</param>
        /// <param name="cancellationtoken"></param>
        /// <param name="callback_downloadProgress">Thông báo tiến trình tải tệp</param>
        /// <param name="callback_getFileNameComplete">Thông báo khhi có tên file</param>
        /// <returns></returns>
        public async Task<string> DownloadFileAsync(string fileid, string folderSaveOnPC, CancellationToken cancellationtoken, Action<double> callback_downloadProgress = null,  Action<string> callback_getFileNameComplete=null)
        {
            //https://developers.google.com/drive/v3/web/manage-downloads
            var fileInfo_request = _service.Files.Get(fileid);

            if (!Directory.Exists(folderSaveOnPC))
                Directory.CreateDirectory(folderSaveOnPC);

            fileInfo_request.Fields = "id, name, size";

            var fileInfo_Response = await fileInfo_request.ExecuteAsync(cancellationtoken);

            if (callback_getFileNameComplete != null)
                callback_getFileNameComplete(fileInfo_Response.Name);

            FileStream fs = new FileStream($"{folderSaveOnPC}\\{fileInfo_Response.Name}", FileMode.Create);

            var download_request = _service.Files.Get(fileInfo_Response.Id);

            if (callback_downloadProgress != null)
                download_request.MediaDownloader.ProgressChanged += 
                    (obj) => callback_downloadProgress((double)obj.BytesDownloaded / fileInfo_Response.Size.Value);

            var download_response = await download_request.DownloadAsync(fs, cancellationtoken);
            
            fs.Close();
            return fs.Name;
        }

        /// <summary>
        /// Tải một tệp về máy
        /// </summary>
        /// <param name="fileid">ID tệp trên drive</param>
        /// <param name="filePathOnPC">Đường dẫn lưu tệp trên máy</param>
        /// <param name="cancellationtoken"></param>
        /// <param name="callback_downloadProgress">Thông báo tiến trình tải lên</param>
        /// <returns></returns>
        public async Task<string> DownloadFileWithCustomFileNameAsync(string fileid, string filePathOnPC, CancellationToken cancellationtoken, Action<double> callback_downloadProgress = null)
        {
            //https://developers.google.com/drive/v3/web/manage-downloads
            var fileInfo_request = _service.Files.Get(fileid);

            fileInfo_request.Fields = "id, name, size";

            var fileInfo_Response = await fileInfo_request.ExecuteAsync(cancellationtoken);

            FileStream fs = new FileStream(filePathOnPC, FileMode.Create);

            var download_request = _service.Files.Get(fileInfo_Response.Id);

            if (callback_downloadProgress != null)
                download_request.MediaDownloader.ProgressChanged +=
                    (obj) => callback_downloadProgress((double)obj.BytesDownloaded / fileInfo_Response.Size.Value);

            var download_response = await download_request.DownloadAsync(fs, cancellationtoken);

            fs.Close();
            return fs.Name;
        }

        /// <summary>
        /// Lấy thông tin tệp
        /// </summary>
        /// <param name="fileid"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public async Task<Google.Apis.Drive.v3.Data.File> GetFileInfoAsync(string fileid, string fields= "id, name, size")
        {
            var fileInfo_request = _service.Files.Get(fileid);

            fileInfo_request.Fields = fields;

            return await fileInfo_request.ExecuteAsync();
        }

        /// <summary>
        /// Tạo thư mục trên drive
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public async Task<string> CreateFolderIfNotExistAync(string folderName)
        {
            #region Tìm kiếm có thì trả về
            var timKiemFolder_request = _service.Files.List();
            timKiemFolder_request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false";
            var timKiemFolder_response = await timKiemFolder_request.ExecuteAsync();
            if (timKiemFolder_response.Files.Count >= 1)
                return timKiemFolder_response.Files[0].Id;
            #endregion

            #region Tạo thư mục
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var request = _service.Files.Create(fileMetadata);

            request.Fields = "id";
            var file = await request.ExecuteAsync();
            return file.Id;
            #endregion
        }

        /// <summary>
        /// Lấy đường dẫn tệp
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public string GetLinkFile(string fileId)
        {
            return $"https://drive.google.com/file/d/{fileId}";
        }
    }
}
