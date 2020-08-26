using System;
using System.Collections.Generic;
using System.Text;

namespace NclVaultCLIClient.Controllers
{
    public class Utils
    {
        public static void PrintBanner()
        {
            string STRING_BANNER = "ICBhZDg4ODg4ODg4ODhiYQogZFAnICAgICAgICAgYCI4YiwKIDggICxhYWEsICAgICAgICJZODg4YSAgICAgLGFhYWEsICAgICAsYWFhLCAgLGFhLAogOCAgOCcgYDggICAgICAgICAgICI4OGJhYWRQIiIiIlliYWFhZFAiIiJZYmRQIiJZYgogOCAgOCAgIDggICAgICAgICAgICAgICIiIiAgICAgICAgIiIiICAgICAgIiIgICAgOGIKIDggIDgsICw4ICAgICAgICAgLGFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWRkZGRkODhQCiA4ICBgIiIiJyAgICAgICAsZDgiIgogWWIsICAgICAgICAgLGFkOCIgICAgIS8vTGFiIFZhdWx0IC0gQW50b25pbyBCbGVzY2lhCiAgIlk4ODg4ODg4ODg4UCIK";

            Console.WriteLine(Environment.NewLine + System.Text.ASCIIEncoding.ASCII.GetString(System.Convert.FromBase64String(STRING_BANNER)));
        }
    }
}