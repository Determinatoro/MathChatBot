﻿using Effort.Provider;
using MathChatBot.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MathChatBot.Test
{
    [TestClass]
    public class UnitTest1
    {
        #region EncryptionUtility

        [TestMethod]       
        public void EncryptUtility_Decrypt_Equal()
        {
            var decryptedText = EncryptUtility.Decrypt("XFRGhQoc3+jj+qQFWhe2vpaRH8q1Pyj1X8NddCoSJcuYXxXisTldPPMq9ynI2doGDQD7ApvSQOJxhK1SDZ0R2c+4pFTR7aVz7Ew3F2UvDpeHNBnOBiiwmMSBq8pF8exs", "test1234");            
            Assert.AreEqual("test1234", decryptedText);
        }

        [TestMethod]
        public void EncryptUtility_Encrypt_Equal()
        {
            var encryptedText = EncryptUtility.Encrypt("test1234", "test1234");
            var decryptedText = EncryptUtility.Decrypt(encryptedText, "test1234");
            Assert.AreEqual("test1234", decryptedText);
        }



        [TestInitialize]
        public void Setup()
        {
            EffortProviderConfiguration.RegisterProvider();
        }
        
        public void SetupContext()
        {
            var connection = Effort.EntityConnectionFactory.CreatePersistent("name=MathChatBotEntities");
            var context = new Models.MathChatBotEntities(connection, true);
        }

        [TestMethod]
        public void TestConnection()
        {
            var connection = Effort.EntityConnectionFactory.CreatePersistent("name=MathChatBotEntities");
            var context = new Models.MathChatBotEntities(connection, true);
        }

        #endregion

    }
}
