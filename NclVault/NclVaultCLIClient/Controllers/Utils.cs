using AutoMapper;
using ConsoleTables;
using NclVaultCLIClient.Attributes;
using NclVaultCLIClient.Models;
using NclVaultFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Text;

namespace NclVaultCLIClient.Controllers
{
    public class Utils
    {
        #region Members
        private static Mapper _mapper;
        #endregion
        public static void PrintBanner()
        {
            string STRING_BANNER = "ICBhZDg4ODg4ODg4ODhiYQogZFAnICAgICAgICAgYCI4YiwKIDggICxhYWEsICAgICAgICJZODg4YSAgICAgLGFhYWEsICAgICAsYWFhLCAgLGFhLAogOCAgOCcgYDggICAgICAgICAgICI4OGJhYWRQIiIiIlliYWFhZFAiIiJZYmRQIiJZYgogOCAgOCAgIDggICAgICAgICAgICAgICIiIiAgICAgICAgIiIiICAgICAgIiIgICAgOGIKIDggIDgsICw4ICAgICAgICAgLGFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWRkZGRkODhQCiA4ICBgIiIiJyAgICAgICAsZDgiIgogWWIsICAgICAgICAgLGFkOCIgICAgIS8vTGFiIFZhdWx0IC0gQW50b25pbyBCbGVzY2lhCiAgIlk4ODg4ODg4ODg4UCIK";

            Console.WriteLine(Environment.NewLine + ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(STRING_BANNER)));
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
            /* Creates a new mapper if null */
            _mapper ??= new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<PasswordEntryReadDto, PrintablePasswordEntryReadDto>()));
            
            /* Maps the PasswordEntryReadDto to  PrintablePasswordEntryReadDto that supports the Printable Attribute*/
            PrintablePasswordEntryReadDto printablePasswordEntryReadDto = _mapper.Map<PrintablePasswordEntryReadDto>(objectToPrint);
            
            /* Extracts the only properties with Printable Attribute */
            List<PropertyInfo> passwordEntryReadDtoPropertiesToPrint = typeof(PrintablePasswordEntryReadDto).GetProperties().Where(p => p.GetCustomAttributes(typeof(Printable), true).Length != 0).ToList();

            /* Sets the header table with the Name of the properties Printable type */
            ConsoleTable helpTable = new ConsoleTable(passwordEntryReadDtoPropertiesToPrint.Select(element => element.Name).ToArray());
            /* Extracts the Property Values and add them as new row  */
            helpTable.AddRow(passwordEntryReadDtoPropertiesToPrint.Select(element => element.GetValue(printablePasswordEntryReadDto)).ToArray());

            helpTable.Write(Format.Alternative);
        }

        public static void PrintEntryTable<T>(List<T> lObjectsToPrint)
        {
            /* Creates a new mapper if null */
            _mapper ??= new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<PasswordEntryReadDto, PrintablePasswordEntryReadDto>()));

            /* Extracts the only properties with Printable Attribute */
            List<PropertyInfo> passwordEntryReadDtoPropertiesToPrint = (typeof(PrintablePasswordEntryReadDto)).GetProperties().Where(p => p.GetCustomAttributes(typeof(Printable), true).Length != 0).ToList();

            /* Sets the header table with the Name of the properties Printable type */
            ConsoleTable helpTable = new ConsoleTable(passwordEntryReadDtoPropertiesToPrint.Select(element => element.Name).ToArray());

            foreach (var objectToPrint in lObjectsToPrint)
            {
                /* Maps the PasswordEntryReadDto to  PrintablePasswordEntryReadDto that supports the Printable Attribute*/
                PrintablePasswordEntryReadDto printablePasswordEntryReadDto = _mapper.Map<PrintablePasswordEntryReadDto>(objectToPrint);
                /* Extracts the Property Values and add them as new row  */
                helpTable.AddRow(passwordEntryReadDtoPropertiesToPrint.Select(element => element.GetValue(printablePasswordEntryReadDto)).ToArray());
            }

            helpTable.Write(Format.Alternative);
        }

        public static IPEndPoint ValidateConnectionProperties(string STRING_IpAddress, string STRING_Port)
        {
            IPAddress IPADDRESS_ParsedIp;
            int INT32_ParsedPort;

            IPEndPoint IPENDPOINT_ParsedConnectionProperties = null;
            if (IPAddress.TryParse(STRING_IpAddress, out IPADDRESS_ParsedIp) && int.TryParse(STRING_Port, out INT32_ParsedPort))
            {
                IPENDPOINT_ParsedConnectionProperties = new IPEndPoint(IPADDRESS_ParsedIp, INT32_ParsedPort);
            }

            return IPENDPOINT_ParsedConnectionProperties;
        }

    }
}