﻿using Furcadia.Logging;
using NUnit.Framework;

namespace FurcadiaLibMono
{
    [SetUpFixture]
    public class OneTimeSetupAndTearDown
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Logger.InfoEnabled = true;
            Logger.SuppressSpam = false;
            Logger.ErrorEnabled = true;
            Logger.WarningEnabled = true;
            Logger.SingleThreaded = true;
            Logger.LogOutput = new MultiLogOutput(new FileLogOutput(Level.Debug), new FileLogOutput(Level.Error));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // TODO: Add code here that is run after
            //  all tests in the assembly have been run
        }
    }
}