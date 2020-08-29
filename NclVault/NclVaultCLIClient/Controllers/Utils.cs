using ConsoleTables;
using NclVaultCLIClient.Attributes;
using NclVaultCLIClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
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

        public static void PrintHelp()
        {
            ConsoleTable helpTable = new ConsoleTable("Command", "Description")
                     .AddRow("/[help|h]", "Shows this table")
                     .AddRow("/[init|i]", "Initialize the database")
                     .AddRow("/[login|l]", "Starts the login procedure")
                     .AddRow("/[readpassword|rp]", "Reads a single password by ID")
                     .AddRow("/[readpasswords|rps]", "Reads all passwords")
                     .AddRow("/[createpassword|cp]", "Creates a new password entry")
                     .AddRow("/exit|quit", "Exit");
            
            helpTable.Write(Format.Alternative);
        }

        public static void PrintEntryTable(PasswordEntryReadDto objectToPrint)
        {
            /* Extracts the only properties with Printable Attribute */
            List<PropertyInfo> passwordEntryReadDtoPropertiesToPrint = objectToPrint.GetType().GetProperties().Where(p => p.GetCustomAttributes(typeof(Printable), true).Length != 0).ToList();

            /* Sets the header table with the Name of the properties Printable type */
            ConsoleTable helpTable = new ConsoleTable(passwordEntryReadDtoPropertiesToPrint.Select(element => element.Name).ToArray());
            /* Extracts the Property Values and add them as new row  */
            helpTable.AddRow(passwordEntryReadDtoPropertiesToPrint.Select(element => element.GetValue(objectToPrint)).ToArray());

            helpTable.Write(Format.Alternative);
        }

        public static void PrintEntryTable<T>(List<T> t)
        {
            
            /* Extracts the only properties with Printable Attribute */
            List<PropertyInfo> passwordEntryReadDtoPropertiesToPrint = (typeof(T)).GetProperties().Where(p => p.GetCustomAttributes(typeof(Printable), true).Length != 0).ToList();

            /* Sets the header table with the Name of the properties Printable type */
            ConsoleTable helpTable = new ConsoleTable(passwordEntryReadDtoPropertiesToPrint.Select(element => element.Name).ToArray());

            foreach (var objectToPrint in t)
            {
                /* Extracts the Property Values and add them as new row  */
                helpTable.AddRow(passwordEntryReadDtoPropertiesToPrint.Select(element => element.GetValue(objectToPrint)).ToArray());
            }
            
            helpTable.Write(Format.Alternative);
        }

        
    }
}