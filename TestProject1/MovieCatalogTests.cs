using MovieCatalog.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using static System.Net.WebRequestMethods;


namespace MovieCatalog
{
    public class Tests
    {
        private RestClient client;
        private static string movieId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("test@test.com", "test123");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient client = new RestClient("http://144.91.123.158:5000");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });
            RestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]
        public void CreateNewMovie()
        {
            MovieDto movie = new MovieDto()
            {
                Title = "test1",
                Description = "test description"
            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(readyResponse.Movie, Is.Not.Null);
            Assert.That(readyResponse.Movie.Id, Is.Not.Null.Or.Empty);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie created successfully!"));
            movieId = readyResponse.Movie.Id;
        }
        [Order(2)]
        [Test]
        public void EditCreatedMovie()
        {

            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", movieId);
            request.AddJsonBody(   new {
                title = "test1 updated",
                description = "test updated",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            });
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));


            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie edited successfully!"));

        }

        [Order(3)]  
        [Test]
        public void GetAllMovies()
        {

            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));


           List<MovieDto> allMovies = JsonSerializer.Deserialize<List<MovieDto>>(response.Content);
           Assert.That(allMovies, Is.Not.Empty);

        }
        [Order(4)]  
        [Test]
        public void DeleteMovie()
        {

            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", movieId);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));


            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Movie deleted successfully!"));

        }
        [Order(5)]
        [Test]
        public void CreateNewMovieWithoutRequiredFields()
        {
            MovieDto movie = new MovieDto()
            {
                Title = "",
                Description = ""
            };

            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(movie);
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        }
        [Order(6)]
        [Test]
        public void EditNonExistantMovie()
        {

            string nonExistantMovieID = "12345";
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", nonExistantMovieID);
            request.AddJsonBody(new
            {
                title = "test",
                description = "test",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            });
            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));

        }
        [Order(7)]
        [Test]

        public void DeleteNonExistantMovie()
        {

            string nonExistantMovieID = "12345";
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", nonExistantMovieID);

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            ApiResponseDto readyResponse = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(readyResponse.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));


        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}
