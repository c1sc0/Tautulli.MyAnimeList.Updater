using System.Threading.Tasks;
using JikanDotNet;
using NUnit.Framework;

namespace malAnimeUpdater.UnitTests
{
    public class Tests
    {
        [Test]
        public async Task Login_IfValidUserCredentialsAreSent_TheUserShouldLoginSuccessfully()
        {
            var malUpdater = new MALService("","","");

            var result = await malUpdater.SetWatchedEpisodeTo(33475, 6);

            Assert.That(result, Is.True);
        }


        [TestCase("Busou Shoujo Machiavellianism", 33475, "2017-04-05")]
        [TestCase("Princess Connect! Re:Dive", 39292, "2020-04-07")]
        [TestCase("Gleipnir", 39463, "2020-04-05")]
        [TestCase("Mob Psycho 100", 50172, "2022-10-20")]
        [TestCase("Reincarnated as a Sword", 49891, "2022-09-28")]
        public async Task GetAnimeId_IfExistingAnimeTitleIsProvided_ReturnAnimeMalId(string title, long malId, string date)
        {
            await Task.Delay(1000);
            var malUpdater = new MALService("","", "");

            var animeId = await malUpdater.GetAnimeId(title, date, "134667");

            Assert.That(animeId, Is.EqualTo(malId));
        }
        

    }
}