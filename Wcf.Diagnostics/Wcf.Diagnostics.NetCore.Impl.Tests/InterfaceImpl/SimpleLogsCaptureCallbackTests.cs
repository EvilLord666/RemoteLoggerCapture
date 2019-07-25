﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using Wcf.Diagnostics.NetCore.Impl.InterfacesImpl;
using Wcf.Diagnostics.NetCore.Impl.Tests.TestUtils;
using Xunit;

namespace Wcf.Diagnostics.NetCore.Impl.Tests.InterfaceImpl
{
    public class SimpleLogsCaptureCallbackTests
    {
        [Fact]
        public void TestLogsCapture()
        {
            // Create server
            WSDualHttpBinding binding = new WSDualHttpBinding();
            binding.Name = "TestService";
            binding.HostNameComparisonMode = HostNameComparisonMode.StrongWildcard;
            binding.Security.Mode = WSDualHttpSecurityMode.None;
            binding.ClientBaseAddress = new Uri(ClientBaseUri);

            ServiceHost serviceHost = new ServiceHost(typeof(TestWcfService), new Uri(ServerBaseUri));
            serviceHost.OpenTimeout = TimeSpan.FromSeconds(10);
            serviceHost.AddServiceEndpoint(typeof(ITestWcfService), binding, "testService");
            /*
            serviceHost.AddServiceEndpoint(//typeof(ITestWcfService),
                                           ServiceMetadataBehavior.MexContractName, 
                                           MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
                //typeof(ITestWcfService), new WSHttpBinding(), "mex");
            
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            smb.HttpGetUrl = new Uri(TestServiceMetadataEndpointUri);
            serviceHost.Description.Behaviors.Add(smb);
            */
            serviceHost.Opened += (sender, args) =>
            {
                System.Console.WriteLine("Service Host Was Opened!");
            };
            
            serviceHost.Open(TimeSpan.FromSeconds(10));
            
            Binding serviceBinding = new WSDualHttpBinding(WSDualHttpSecurityMode.None);
            EndpointAddress endpointAddress = new EndpointAddress(TestServiceEndpointUri);
            // client channel creation
            DuplexChannelFactory<ITestWcfService> channelFactory = new DuplexChannelFactory<ITestWcfService>(new SimpleLogsCaptureCallback("..", true, new[] {"*.log"}),
                                                                                                             serviceBinding, endpointAddress);
            ITestWcfService client = channelFactory.CreateChannel();

            int sessionId = client.LogIn("MyDomain", "admin", "123");

            Assert.True(sessionId < 1000);
            
            // todo: umv: implement logs capture from client here ...

            bool result = client.LogOut(sessionId);
            
            Assert.True(result);
            serviceHost.Close();
        }

        private const string ServerBaseUri = "http://127.0.0.1:8000/";
        private const string ClientBaseUri = "http://127.0.0.1:8008/";
        private const string TestServiceEndpointUri = "http://127.0.0.1:8000/testService/";
        // private const string TestServiceMetadataEndpointUri = "http://127.0.0.1:8000/mex/";
    }
}
