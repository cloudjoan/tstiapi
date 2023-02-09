using SAP.Middleware.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TSTI_API.Controllers
{
    public class SapConnector
    {

        public RfcDestination getCRM_PRD()
        {
            SapConnector sapConnector = new SapConnector();
            return sapConnector.nco("PRD");
        }

        public RfcDestination getCRM_QAS()
        {
            SapConnector sapConnector = new SapConnector();
            return sapConnector.nco("QAS");
        }

        public RfcDestination getCRM_DEV()
        {
            SapConnector sapConnector = new SapConnector();
            return sapConnector.nco("DEV");
        }

        public RfcDestination getTatung_PRD()
        {
            SapConnector sapConnector = new SapConnector();
            return sapConnector.nco("TatungPRD");
        }

        public RfcDestination getTatung_QAS()
        {
            SapConnector sapConnector = new SapConnector();
            return sapConnector.nco("TatungQAS");
        }

        public class MyBackendConfig : IDestinationConfiguration
        {



            public RfcConfigParameters GetParameters(String destinationName)
            {
                #region DEV測試
                if ("DEV".Equals(destinationName))
                {
                    RfcConfigParameters parms = new RfcConfigParameters();

                    parms.Add(RfcConfigParameters.AppServerHost, "172.31.7.111");   //SAP主机IP
                    parms.Add(RfcConfigParameters.SystemNumber, "00");  //SAP实例
                    parms.Add(RfcConfigParameters.User, "RFCUSER2");  //用户名
                    parms.Add(RfcConfigParameters.Password, "70771557");  //密码
                    parms.Add(RfcConfigParameters.Client, "400");  // Client
                    parms.Add(RfcConfigParameters.Language, "ZF");  //登陆语言
                    parms.Add(RfcConfigParameters.PoolSize, "5");
                    parms.Add(RfcConfigParameters.MaxPoolSize, "10");
                    parms.Add(RfcConfigParameters.IdleTimeout, "60");

                    return parms;
                }
                else if ("QAS".Equals(destinationName))
                {
                    RfcConfigParameters parms = new RfcConfigParameters();

                    parms.Add(RfcConfigParameters.AppServerHost, "172.31.7.112");   //SAP主机IP
                    parms.Add(RfcConfigParameters.SystemNumber, "00");  //SAP实例
                    parms.Add(RfcConfigParameters.User, "RFCUSER");  //用户名 RFCUSER
                    parms.Add(RfcConfigParameters.Password, "welcome");  //密码 welcome
                    parms.Add(RfcConfigParameters.Client, "810");  // Client
                    parms.Add(RfcConfigParameters.Language, "ZF");  //登陆语言
                    parms.Add(RfcConfigParameters.PoolSize, "5");
                    parms.Add(RfcConfigParameters.MaxPoolSize, "10");
                    parms.Add(RfcConfigParameters.IdleTimeout, "60");

                    return parms;
                }
                else if ("PRD".Equals(destinationName)) //PRD正式區
                {
                    RfcConfigParameters parms = new RfcConfigParameters();

                    parms.Add(RfcConfigParameters.AppServerHost, "172.31.7.113");   //SAP主机IP
                    parms.Add(RfcConfigParameters.SystemNumber, "00");  //SAP实例
                    parms.Add(RfcConfigParameters.User, "CRMSRVUSER");  //用户名
                    parms.Add(RfcConfigParameters.Password, "welcome");  //密码
                    parms.Add(RfcConfigParameters.Client, "880");  // Client
                    parms.Add(RfcConfigParameters.Language, "ZF");  //登陆语言
                    parms.Add(RfcConfigParameters.PoolSize, "5");
                    parms.Add(RfcConfigParameters.MaxPoolSize, "10");
                    parms.Add(RfcConfigParameters.IdleTimeout, "60");

                    return parms;
                }
                else if ("TatungQAS".Equals(destinationName)) //QAS測試區(for大同)
                {
                    RfcConfigParameters parms = new RfcConfigParameters();

                    parms.Add(RfcConfigParameters.AppServerHost, "qas10.tatung.com");   //SAP主机IP
                    parms.Add(RfcConfigParameters.SystemNumber, "00");  //SAP实例
                    parms.Add(RfcConfigParameters.User, "TWP10RFC21");  //用户名
                    parms.Add(RfcConfigParameters.Password, "p10rfc21tw");  //密码
                    parms.Add(RfcConfigParameters.Client, "588");  // Client
                    parms.Add(RfcConfigParameters.Language, "ZF");  //登陆语言
                    parms.Add(RfcConfigParameters.PoolSize, "5");
                    parms.Add(RfcConfigParameters.MaxPoolSize, "10");
                    parms.Add(RfcConfigParameters.IdleTimeout, "160");

                    return parms;
                }
                else if ("TatungPRD".Equals(destinationName)) //PRD正式區(for大同)
                {
                    RfcConfigParameters parms = new RfcConfigParameters();

                    parms.Add(RfcConfigParameters.AppServerHost, "139.223.3.128");   //SAP主机IP
                    //parms.Add(RfcConfigParameters.AppServerHost, "sap21.tatung.com");   //SAP主机DNS
                    parms.Add(RfcConfigParameters.SystemNumber, "00");  //SAP实例
                    parms.Add(RfcConfigParameters.User, "TWP10RFC21");  //用户名
                    parms.Add(RfcConfigParameters.Password, "p10rfc21tw");  //密码
                    parms.Add(RfcConfigParameters.Client, "888");  // Client
                    parms.Add(RfcConfigParameters.Language, "ZF");  //登陆语言
                    parms.Add(RfcConfigParameters.PoolSize, "5");
                    parms.Add(RfcConfigParameters.MaxPoolSize, "10");
                    parms.Add(RfcConfigParameters.IdleTimeout, "160");

                    return parms;
                }
                else
                {
                    return null;
                }
                #endregion
            }

            public bool ChangeEventsSupported()
            {
                return false;
            }

            public event RfcDestinationManager.ConfigurationChangeHandler ConfigurationChanged;
        }

        public RfcDestination nco(string serverName)
        {
            IDestinationConfiguration ID = new MyBackendConfig();
            RfcDestinationManager.RegisterDestinationConfiguration(ID);
            RfcDestination sap = RfcDestinationManager.GetDestination(serverName);
            RfcDestinationManager.UnregisterDestinationConfiguration(ID);

            return sap;
        }
    }
}