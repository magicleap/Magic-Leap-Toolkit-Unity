// ---------------------------------------------------------------------
//
// Copyright (c) 2018-present, Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Creator Agreement, located
// here: https://id.magicleap.com/terms/developer
//
// ---------------------------------------------------------------------

using System.Net;
using System.Net.Sockets;

namespace MagicLeapTools
{
    public static class NetworkUtilities
    {
        //Public Properties:
        public static string MyAddress
        {
            get
            {
                if (string.IsNullOrEmpty(_address))
                {
                    string hostName = Dns.GetHostName();

                    IPAddress[] ip = Dns.GetHostEntry(hostName).AddressList;

                    foreach (var item in ip)
                    {
                        if (item.AddressFamily == AddressFamily.InterNetwork)
                        {
                            _address = item.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(_address))
                    {
                        _address = "0.0.0.0";
                    }
                }

                return _address;
            }
        }

        //Private Variables:
        private static string _address;
    }
}