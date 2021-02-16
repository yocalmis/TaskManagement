using GorevYoneticisi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace GorevYoneticisi.Tools
{
    public class SendSms
    {
        // Ayarlar
        public string User = null;//"5542148366";
        public string Pass = null;//"Tb707718";
        public DateTime sonGuncelleme;

        // Adresler
        public string UrlSmsGet = "http://api.iletimerkezi.com/v1/send-sms/get/";
        public string UrlSmsPost = "http://api.iletimerkezi.com/v1/send-sms";
        public string UrlGetReport = "http://api.iletimerkezi.com/v1/get-report";
        public string UrlGetBalance = "http://api.iletimerkezi.com/v1/get-balance";
        public string UrlCancelOrder = "http://api.iletimerkezi.com/v1/cancel-order";

        // Sunucu Cevabı
        public string ServerResponse = "";

        // Durum Kodları
        public int StatusCode = 0;
        public string StatusDesc = "";

        // Bakiye
        public int BalanceSmsCount = 0;
        public double BalanceAmount = 0.0;

        // Order
        public int OrderId = 0;

        // Sms
        public string SmsBody = "";
        public string SmsOriginator = "GOREV YONET";

        // SqlConnection
        //SqlConnection SqlConn;

        public static int smsHakkiSorgula(string musteri_no)
        {
            var response = new WebClient().DownloadString("https://www.musavire-destek.com/satis-destek/services/index/" + musteri_no);

            int smsSayisi = 0;
            if (!response.Equals(string.Empty))
            {
                smsSayisi = Convert.ToInt32(response);
            }

            return smsSayisi;
        }
        public static bool smsHakkiEksilt(string musteri_no, int ilkBakiye, int sonBakiye)
        {

            //çalışmayan link = https://www.musavire-destek.com/satis-destek/services/son_bakiye/330548/999/521156f07b9bc885000c84c5e9d256f6

            string sifre = "zf-rt-43-ert-567-fwe";
            //md5($musteri_no."_".$bakiye."_".$son_bakiye."_".$sifre);
            string anahtar = MD5Hash.GetMd5Hash(musteri_no + "_" + ilkBakiye + "_" + sonBakiye + "_" + sifre);
            var response = new WebClient().DownloadString("https://www.musavire-destek.com/satis-destek/services/son_bakiye/" + musteri_no + "/" + sonBakiye + "/" + anahtar);

            bool sonuc = false;
            if (!response.Equals(string.Empty))
            {
                sonuc = Convert.ToBoolean(response);
            }

            return sonuc;
        }
        public SendSms()
        { }
        public SendSms(string user, string pass, string originator)
        {
            this.User = user;
            this.Pass = pass;
            this.SmsOriginator = originator;
        }

        public bool SendSMS(string[] Recipents, string SmsText, string smsHeader, string musteri_no)
        {
            try
            {
                int smsSayisi = 0;
                if (!musteri_no.Equals("_admin_"))
                {
                    smsSayisi = smsHakkiSorgula(musteri_no);
                    if (Recipents.Count() > smsSayisi)
                    {
                        return false;
                    }
                }
                
                if (User == null || DateTime.Now.AddMinutes(2) >= sonGuncelleme)
                {
                    vrlfgysdbEntities db = new vrlfgysdbEntities();
                    sistem_ayarlari sa = db.sistem_ayarlari.Where(e => e.flag == durumlar.aktif).FirstOrDefault();
                    User = sa.sms_username;
                    Pass = sa.sms_password;
                    sonGuncelleme = DateTime.Now;
                }
                if (smsHeader == null || smsHeader.Equals(string.Empty))
                {
                    vrlfgysdbEntities db = new vrlfgysdbEntities();
                    sistem_ayarlari sa = db.sistem_ayarlari.Where(e => e.flag == durumlar.aktif).FirstOrDefault();
                    smsHeader = sa.sms_header;
                }
                // Prepare
                this.SmsBody = SmsText;

                // Send Via Get
                // SendSmsViaGet(this.SmsBody, Recipents);

                // Send Via Post
                SendSmsViaPost(this.SmsBody, Recipents, smsHeader);

                bool sonuc = (this.StatusCode == 200 || this.StatusCode == 451);
                // Return
                if (sonuc)
                {
                    int sonBakiye = smsSayisi - Recipents.Count();
                    smsHakkiEksilt(musteri_no, smsSayisi, sonBakiye);
                }
                return sonuc;
            }
            catch
            {
                return false;
            }
        }

        /*private void SendSmsViaGet(string SmsText, string[] Recipents)
        {
            // Reset Status
            this.StatusCode = 0;
            this.SetStatus();

            // Basic GET Request
            this.DoRequest(UrlSmsGet, "GET", Uri.EscapeUriString("username=" + User + "&password=" + Pass + "&text=" + SmsText + "&receipents=" + String.Join(",", Recipents) + "&sender=" + this.SmsOriginator));

            // Console
            Console.WriteLine(this.ServerResponse);

        }*/

        private void SendSmsViaPost(string SmsText, string[] Recipents, string smsHeader)
        {
            // Reset Status
            this.StatusCode = 0;
            this.SetStatus();

            // Prepare POST Request Xml
            string xmlRequest = "";
            xmlRequest += "<request>";
            xmlRequest += "  <authentication>";
            xmlRequest += "    <username>" + this.User + "</username>";
            xmlRequest += "    <password>" + this.Pass + "</password>";
            xmlRequest += "  </authentication>";
            xmlRequest += "  <order>";
            xmlRequest += "    <sender>" + smsHeader + "</sender>";
            // xmlRequest += "    <sendDateTime></sendDateTime>"; // GG/AA/YYYY SS:DD
            xmlRequest += "    <message>";
            xmlRequest += "      <text><![CDATA[" + SmsText + "]]></text>";
            xmlRequest += "      <receipents>";
            for (int i = 0; i < Recipents.Length; i++)
                xmlRequest += "        <number>" + Recipents[i] + "</number>";
            xmlRequest += "      </receipents>";
            xmlRequest += "    </message>";
            xmlRequest += "  </order>";
            xmlRequest += "</request>";

            // Do POST Request
            this.DoRequest(UrlSmsPost, "POST", xmlRequest);

            string sonuc = this.ServerResponse;

            // Parse Xml
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(this.ServerResponse);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/response/status");
            foreach (XmlNode node in nodeList)
            {
                this.StatusCode = int.Parse(node.SelectSingleNode("code").InnerText);
                this.StatusDesc = node.SelectSingleNode("message").InnerText;
            }

            if (this.StatusCode == 200)
            {
                nodeList = xmlDoc.DocumentElement.SelectNodes("/response/order");
                foreach (XmlNode node in nodeList)
                {
                    this.OrderId = int.Parse(node.SelectSingleNode("id").InnerText);
                }
            }

            // Console
            //Console.WriteLine(this.ServerResponse);
        }

        /*public void CancelOrder()
        {
            // Reset Status
            this.StatusCode = 0;
            this.SetStatus();

            // Prepare POST Request Xml
            string xmlRequest = "";
            xmlRequest += "<request>";
            xmlRequest += "  <authentication>";
            xmlRequest += "    <username>" + this.User + "</username>";
            xmlRequest += "    <password>" + this.Pass + "</password>";
            xmlRequest += "  </authentication>";
            xmlRequest += "</request>";

            // Do POST Request
            this.DoRequest(UrlCancelOrder, "POST", xmlRequest);

            // Parse Xml
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(this.ServerResponse);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/response/status");
            foreach (XmlNode node in nodeList)
            {
                this.StatusCode = int.Parse(node.SelectSingleNode("code").InnerText);
                this.StatusDesc = node.SelectSingleNode("message").InnerText;
            }
        }*/

        /*public void GetBalance()
        {
            // Reset Status
            this.StatusCode = 0;
            this.SetStatus();

            // Prepare POST Request Xml
            string xmlRequest = "";
            xmlRequest += "<request>";
            xmlRequest += "  <authentication>";
            xmlRequest += "    <username>" + this.User + "</username>";
            xmlRequest += "    <password>" + this.Pass + "</password>";
            xmlRequest += "  </authentication>";
            xmlRequest += "</request>";

            // Do POST Request
            this.DoRequest(UrlGetBalance, "POST", xmlRequest);

            // Parse Xml
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(this.ServerResponse);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/response/status");
            foreach (XmlNode node in nodeList)
            {
                this.StatusCode = int.Parse(node.SelectSingleNode("code").InnerText);
                this.StatusDesc = node.SelectSingleNode("message").InnerText;
            }

            if (this.StatusCode == 200)
            {
                nodeList = xmlDoc.DocumentElement.SelectNodes("/response/balance");
                foreach (XmlNode node in nodeList)
                {
                    this.BalanceSmsCount = int.Parse(node.SelectSingleNode("sms").InnerText);
                    this.BalanceAmount = Double.Parse(node.SelectSingleNode("amount").InnerText);
                }
            }
        }*/

        /*public void GetReport(int orderId, int pageNumber = 1, int rowCount = 1000)
        {
            // Reset Status
            this.StatusCode = 0;
            this.SetStatus();

            // Prepare POST Request Xml
            string xmlRequest = "";
            xmlRequest += "<request>";
            xmlRequest += "  <authentication>";
            xmlRequest += "    <username>" + this.User + "</username>";
            xmlRequest += "    <password>" + this.Pass + "</password>";
            xmlRequest += "  </authentication>";
            xmlRequest += "  <order>";
            xmlRequest += "    <id>" + orderId + "</id>";
            xmlRequest += "    <page>" + pageNumber + "</page>";
            xmlRequest += "    <rowCount>" + rowCount + "</rowCount>";
            xmlRequest += "  </order>";
            xmlRequest += "</request>";

            // Do POST Request
            this.DoRequest(UrlGetReport, "POST", xmlRequest);
            Console.WriteLine(this.ServerResponse);
            // Parse Xml
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(this.ServerResponse);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/response/status");
            foreach (XmlNode node in nodeList)
            {
                this.StatusCode = int.Parse(node.SelectSingleNode("code").InnerText);
                this.StatusDesc = node.SelectSingleNode("message").InnerText;
            }


            if (this.StatusCode == 200)
            {

            }

            // Console
            Console.WriteLine(this.ServerResponse);
        }*/

        private void SetStatus()
        {
            switch (this.StatusCode)
            {
                case 0:
                    this.StatusDesc = "";
                    break;
                case 2:
                    this.StatusDesc = "";
                    break;
                case 110:
                    this.StatusDesc = "Mesaj gönderiliyor";
                    break;
                case 111:
                    this.StatusDesc = "Mesaj gönderildi";
                    break;
                case 112:
                    this.StatusDesc = "Mesaj gönderilemedi";
                    break;
                case 113:
                    this.StatusDesc = "Siparişin gönderimi devam ediyor";
                    break;
                case 114:
                    this.StatusDesc = "Siparişin gönderimi tamamlandı";
                    break;
                case 115:
                    this.StatusDesc = "Sipariş gönderilemedi";
                    break;
                case 200:
                    this.StatusDesc = "İşlem başarılı";
                    break;
                case 400:
                    this.StatusDesc = "İstek çözümlenemedi";
                    break;
                case 401:
                    this.StatusDesc = "Üyelik bilgileri hatalı";
                    break;
                case 402:
                    this.StatusDesc = "Bakiye yetersiz";
                    break;
                case 404:
                    this.StatusDesc = "API istek yapılan yönteme sahip değil ";
                    break;
                case 450:
                    this.StatusDesc = "Gönderilen başlık kullanıma uygun değil";
                    break;
                case 451:
                    this.StatusDesc = "Tekrar eden sipariş";
                    break;
                case 452:
                    this.StatusDesc = "Mesaj alıcıları hatalı";
                    break;
                case 453:
                    this.StatusDesc = "Sipariş boyutu aşıldı";
                    break;
                case 454:
                    this.StatusDesc = "Mesaj metni boş";
                    break;
                case 455:
                    this.StatusDesc = "Sipariş bulunamadı";
                    break;
                case 456:
                    this.StatusDesc = "Sipariş gönderim tarihi henüz gelmedi";
                    break;
                case 457:
                    this.StatusDesc = "Mesaj gönderim tarihinin formatı hatalı";
                    break;
                case 503:
                    this.StatusDesc = "Sunucu geçici olarak servis dışı";
                    break;
                default:
                    this.StatusDesc = "";
                    break;
            }
        }

        private void DoRequest(string requestUrl, string requestMethod, string requestData)
        {
            if (requestMethod == "GET")
            {
                try
                {
                    // Create a web request for an invalid site. Substitute the "invalid site" strong in the Create call with a invalid name.
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl + "?" + requestData);

                    // Get the associated response for the above request.
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    // Check StatusCode
                    if (response.StatusCode.ToString() == "OK") StatusCode = 200;

                    // Close
                    response.Close();
                }
                catch (WebException e)
                {
                    StatusCode = int.Parse(Between(e.Message, "(", ")"));
                }
                catch (Exception e)
                {
                    StatusCode = 0;
                }
                finally
                {
                    SetStatus();
                }
            }
            else if (requestMethod == "POST")
            {
                try
                {
                    // Create a request using a URL that can receive a post.
                    WebRequest request = WebRequest.Create(requestUrl);

                    // Set the Method property of the request to POST.
                    request.Method = requestMethod;

                    // Create POST data and convert it to a byte array.
                    string postData = requestData;
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    // Set the ContentType property of the WebRequest.
                    request.ContentType = "application/x-www-form-urlencoded";

                    // Set the ContentLength property of the WebRequest.
                    request.ContentLength = byteArray.Length;

                    // Get the request stream.
                    Stream dataStream = request.GetRequestStream();

                    // Write the data to the request stream.
                    dataStream.Write(byteArray, 0, byteArray.Length);

                    // Close the Stream object.
                    dataStream.Close();

                    // Get the response.
                    WebResponse response = request.GetResponse();

                    // Display the status.
                    Console.WriteLine(((HttpWebResponse)response).StatusDescription);

                    // Get the stream containing content returned by the server.
                    dataStream = response.GetResponseStream();

                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);

                    // Read the content.
                    this.ServerResponse = reader.ReadToEnd();

                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                }
                catch (WebException e)
                {
                    StatusCode = int.Parse(Between(e.Message, "(", ")"));
                }
                catch (Exception e)
                {
                    StatusCode = 0;
                }
                finally
                {
                    SetStatus();
                }
            }

        }

        private string Between(string Source, string firstString, string lastString)
        {
            int posA = Source.IndexOf(firstString) + firstString.Length;
            if (posA > Source.Length) return "";
            string temp = Source.Substring(posA);
            int posB = posA + temp.IndexOf(lastString);

            if (posA == -1) return "";
            if (posB == -1) return "";
            if (posA >= posB) return "";

            string FinalString = Source.Substring(posA, posB - posA);
            return FinalString;
        }

        public static int getGroupId()
        {
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            int groupId = 1;
            if (db.smsler.Count() != 0)
            {
                groupId = db.smsler.Max(e => e.sms_grup_id) + 1;
            }
            return groupId;
        }
        public static void smsKaydet(string mesaj, int flag, int mailHedefTur, int hedefId, string hedef_numara, int gonderenId, int smsGroupId)
        {
            LoggedUserModel lgm = GetCurrentUser.GetUser();
            vrlfgysdbEntities db = new vrlfgysdbEntities();
            int vid = 1;
            if (db.smsler.Count() != 0)
            {
                vid = db.smsler.Max(e => e.vid) + 1;
            }
            int sort = 1;
            if (db.smsler.Count() != 0)
            {
                sort = db.smsler.Max(e => e.sort) + 1;
            }

            smsler sms = new smsler();
            sms.flag = flag;
            sms.date = DateTime.Now;
            sms.icerik = mesaj;
            sms.vid = vid;
            sms.gonderen_id = gonderenId;
            sms.hedef_id = hedefId;
            sms.hedef_numara = hedef_numara;
            sms.hedef_tur = mailHedefTur;
            sms.sms_grup_id = smsGroupId;
            sms.sort = sort;
            sms.firma_id = lgm.firma_id;

            string strImageName = StringFormatter.OnlyEnglishChar(Tools.OurFunctions.ourSubString(sms.icerik,15));
            string createdUrl = strImageName;
            string tempUrl = createdUrl;
            bool bulundu = false;
            int i = 0;
            smsler pg = new smsler();
            do
            {
                pg = db.smsler.Where(e => e.url.Equals(tempUrl)).FirstOrDefault();
                if (pg != null)
                {
                    tempUrl = tempUrl + i.ToString();
                }
                else
                {
                    createdUrl = tempUrl;
                    bulundu = true;
                }
                i++;
            } while (!bulundu);
            sms.url = createdUrl;

            db.smsler.Add(sms);
            db.SaveChanges();
        }

        /*public void Log(SqlConnection sqlConn, int SenderUserId, int[] RecipientsUserId, string[] RecipientsPhone, string SmsType, string SmsCode, bool CloseConnection = true)
        {
            // Set Connection
            this.SqlConn = sqlConn;

            // Open Connection
            if (this.SqlConn != null && SqlConn.State == ConnectionState.Closed) SqlConn.Open();

            // Sql Command
            SqlCommand sqlCmd = new SqlCommand("" +
            "INSERT INTO SmsLog (Sender,  RecipientsUserId,  RecipientsPhone,  SmsOriginator,  SmsType,  SmsBody,  SmsCode,  SendAt,  SendedVia,  ResponseStatus,  ResponseOrderId,  CreatedAt,  CreatedBy,  UpdatedAt,  UpdatedBy) " +
            "VALUES             (@Sender, @RecipientsUserId, @RecipientsPhone, @SmsOriginator, @SmsType, @SmsBody, @SmsCode, @SendAt, @SendedVia, @ResponseStatus, @ResponseOrderId, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy) "
            , this.SqlConn);
            sqlCmd.Parameters.AddWithValue("@Sender", SenderUserId);
            sqlCmd.Parameters.AddWithValue("@RecipientsUserId", String.Join(",", RecipientsUserId));
            sqlCmd.Parameters.AddWithValue("@RecipientsPhone", String.Join(",", RecipientsPhone));
            sqlCmd.Parameters.AddWithValue("@SmsOriginator", this.SmsOriginator);
            sqlCmd.Parameters.AddWithValue("@SmsType", SmsType);
            sqlCmd.Parameters.AddWithValue("@SmsBody", this.SmsBody);
            sqlCmd.Parameters.AddWithValue("@SmsCode", SmsCode);
            sqlCmd.Parameters.AddWithValue("@SendAt", DateTime.Now);
            sqlCmd.Parameters.AddWithValue("@SendedVia", "IletiMerkezi");
            sqlCmd.Parameters.AddWithValue("@ResponseStatus", this.StatusCode);
            sqlCmd.Parameters.AddWithValue("@ResponseOrderId", this.OrderId);
            sqlCmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
            sqlCmd.Parameters.AddWithValue("@CreatedBy", SenderUserId);
            sqlCmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now);
            sqlCmd.Parameters.AddWithValue("@UpdatedBy", SenderUserId);
            sqlCmd.ExecuteNonQuery();

            // Close Connection
            if (CloseConnection) SqlConn.Close();

        }

    }*/


    /*public static bool send(List<string> telefonListesi, string mesaj)
    {
        string numaralar = "";
        for (int i = 0; i < telefonListesi.Count; i++)
        {
            if (telefonListesi[i].Length != 0)
            {
                if (!(telefonListesi[i].Substring(0, 2).Equals("05") || telefonListesi[i].Substring(0, 1).Equals("5") || telefonListesi[i].Substring(0, 3).Equals("(05") || telefonListesi[i].Substring(0, 2).Equals("(5")))
                {
                    telefonListesi[i] = "";
                }
                else
                {
                    if (telefonListesi[i].Substring(0, 1).Equals("("))
                    {
                        telefonListesi[i] = telefonListesi[i].Remove(0, 1);
                        telefonListesi[i] = telefonListesi[i].Replace(")", "");
                        telefonListesi[i] = telefonListesi[i].Replace(" ", "");
                        if (!numaralar.Contains(telefonListesi[i]))
                        {
                            numaralar += "<number>" + telefonListesi[i] + "</number>";
                        }
                    }
                    else
                    {
                        telefonListesi[i] = telefonListesi[i].Replace(")", "");
                        telefonListesi[i] = telefonListesi[i].Replace(" ", "");
                        if (!numaralar.Contains(telefonListesi[i]))
                        {
                            numaralar += "<number>" + telefonListesi[i] + "</number>";
                        }
                    }
                }
            }
        }
        String messageXml = "<request>";
        messageXml += "<authentication>";
        messageXml += "<username>5542148366</username>";
        messageXml += "<password>zirvesms1.</password>";
        messageXml += "</authentication>";
        messageXml += "<order>";
        messageXml += "<sender>GorevYoneticisi</sender>";
        messageXml += "<sendDateTime></sendDateTime>";
        messageXml += "<message>";
        messageXml += "<text><![CDATA[" + mesaj + "]]></text>";
        messageXml += "<receipents>";
        messageXml += numaralar;
        messageXml += "</receipents>";
        messageXml += "</message>";
        messageXml += "</order>";
        messageXml += "</request>";

        string PostAddress = "http://api.iletimerkezi.com/v1/send-sms";

        try
        {
            var res = "";
            byte[] bytes = Encoding.UTF8.GetBytes(messageXml);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PostAddress);

            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "text/xml";
            request.Timeout = 300000000;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            // This sample only checks whether we get an "OK" HTTP status code back.
            // If you must process the XML-based response, you need to read that from
            // the response stream.
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format(
                    "POST failed. Received HTTP {0}",
                    response.StatusCode);
                    throw new ApplicationException(message);
                }

                Stream responseStream = response.GetResponseStream();
                using (StreamReader rdr = new StreamReader(responseStream))
                {
                    res = rdr.ReadToEnd();
                }
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }*/
    }
}