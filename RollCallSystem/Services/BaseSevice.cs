using Microsoft.Ajax.Utilities;
using RollCallSystem.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace RollCallSystem.Services
{
    public class BaseSevice:IDisposable
    {
        protected RCSContext RCSContext;
        protected DetectService DetectService;
        protected MD5 md5Hash;
        protected readonly Guid guid;

        public BaseSevice()
        {
            RCSContext = new RCSContext();
            DetectService = DetectService.Instance;
            md5Hash = MD5.Create();
        }
        protected byte[] parseToMD5Byte(string str)
        {
            var a = MD5Hash.GetMd5Hash(md5Hash, str);
            return Encoding.Unicode.GetBytes(str);
        }
        protected bool compareByteArray(byte[] arr1, byte[] arr2)
        {
            if (arr1 == null || arr2 == null)
                return false;
            else if (arr2.Length != arr1.Length)
                return false;
            else
            {
                for (int i = 0; i < arr1.Length; i++)
                {
                    if (arr1[i] != arr2[i]) return false;
                }
            }
            return true;
        }
        protected int getCurrentUserId()
        {
            var claims = ClaimsPrincipal.Current.Claims;
            var id = Int32.Parse(claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value);
            return id;
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose");
        }
    }
}