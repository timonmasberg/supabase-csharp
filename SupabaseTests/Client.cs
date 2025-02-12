﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Supabase;
using Supabase.Realtime;
using SupabaseTests.Models;
using static Supabase.Client;

namespace SupabaseTests
{
    [TestClass]
    public class Client
    {
        private string password = "I@M@SuperP@ssWord";

        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [TestInitialize]
        public async Task InitializeTest()
        {
            await InitializeAsync("http://localhost", null, new Supabase.SupabaseOptions
            {
                AuthUrlFormat = "{0}:9999",
                RealtimeUrlFormat = "{0}:4000/socket",
                RestUrlFormat = "{0}:3000",
                ShouldInitializeRealtime = true,
                AutoConnectRealtime = true
            });
        }

        [TestMethod("Client: Initializes.")]
        public void ClientInitializes()
        {
            Assert.IsNotNull(Instance.Realtime);
            Assert.IsNotNull(Instance.Auth);
        }

        //[TestMethod("Client: Connects to Realtime")]
        //public async Task ClientConnectsToRealtime()
        //{
        //    var tsc = new TaskCompletionSource<bool>();

        //    var email = $"{RandomString(12)}@supabase.io";
        //    await Instance.Auth.SignUp(email, password);

        //    await Instance.Realtime.ConnectAsync();

        //    var channel = Instance.Realtime.Channel("realtime", "public", "channels");

        //    channel.StateChanged += (sender, ev) =>
        //    {
        //        if (ev.State == Supabase.Realtime.Channel.ChannelState.Joined)
        //            tsc.SetResult(true);
        //    };

        //    await channel.Subscribe();

        //    var result = await tsc.Task;
        //    Assert.IsTrue(result);
        //}

        [TestMethod("SupabaseModel: Successfully Updates")]
        public async Task SupabaseModelUpdates()
        {
            var model = new Models.Channel { Slug = Guid.NewGuid().ToString() };
            var insertResult = await Instance.From<Models.Channel>().Insert(model);
            var newChannel = insertResult.Models.FirstOrDefault();

            var newSlug = $"Updated Slug @ {DateTime.Now.ToLocalTime()}";
            newChannel.Slug = newSlug;

            var updatedResult = await newChannel.Update<Models.Channel>();

            Assert.AreEqual(newSlug, updatedResult.Models.First().Slug);
        }

        [TestMethod("SupabaseModel: Successfully Deletes")]
        public async Task SupabaseModelDeletes()
        {
            var slug = Guid.NewGuid().ToString();
            var model = new Models.Channel { Slug = slug };

            var insertResult = await Instance.From<Models.Channel>().Insert(model);
            var newChannel = insertResult.Models.FirstOrDefault();

            await newChannel.Delete<Models.Channel>();

            var result = await Instance.From<Models.Channel>().Filter("slug", Postgrest.Constants.Operator.Equals, slug).Get();

            Assert.AreEqual(0, result.Models.Count);
        }
    }
}
