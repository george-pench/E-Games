﻿using AutoMapper;
using E_Games.Common;
using E_Games.Common.DTOs;
using E_Games.Data.Data;
using E_Games.Data.Data.Enums;
using E_Games.Data.Data.Models;
using E_Games.Services.E_Games.Services;
using E_Games.Web.Mapping;
using Microsoft.EntityFrameworkCore;

namespace E_Games.Tests
{
    public class GameServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly GameService _gameService;
        private readonly IMapper _mapper;

        public GameServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            var mockMapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfile());
            });

            _mapper = mockMapper.CreateMapper();

            _gameService = new GameService(_context, null!, _mapper);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var products = new List<Product>
        {
            new Product { Id = 1, Name = "The Witcher 3: Wild Hunt", Platform = Platforms.PC },
            new Product { Id = 2, Name = "Red Dead Redemption 2", Platform = Platforms.Nintendo },
            new Product { Id = 3, Name = "Grand Theft Auto V", Platform = Platforms.Xbox },
            new Product { Id = 4, Name = "The Legend of Zelda", Platform = Platforms.Xbox },
            new Product { Id = 5, Name = "Dark Souls III", Platform = Platforms.PC },
            new Product { Id = 6, Name = "The Elder Scrolls V: Skyrim", Platform = Platforms.Nintendo },
            new Product { Id = 7, Name = "Sekiro: Shadows Die Twice", Platform = Platforms.Xbox },
            new Product { Id = 8, Name = "Bloodborne", Platform = Platforms.PC },
            new Product { Id = 9, Name = "Mass Effect 2", Platform = Platforms.Xbox },
            new Product { Id = 10, Name = "Hollow Knight", Platform = Platforms.Mobile },
            new Product { Id = 11, Name = "The Last of Us Part II", Platform = Platforms.PC },
        };

            _context.Products.AddRange(products);
            _context.SaveChanges();
        }

        [Fact]
        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetProductByIdAsync_ReturnsCorrectProduct_WhenProductExists()
        {
            // Arrange
            int productId = 1;

            // Act
            var result = await _gameService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.Id);
            Assert.Equal("The Witcher 3: Wild Hunt", result.Name);
        }

        [Fact]
        public async Task GetProductByIdAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            int nonExistentProductId = 99;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiExceptionBase>(
                () => _gameService.GetProductByIdAsync(nonExistentProductId));
            Assert.Equal("Not Found", exception.Message);
        }

        [Fact]
        public async Task CreateProductAsync_AddsNewProduct_WithCorrectData()
        {
            // Arrange
            var newProductDto = new CreateProductDto
            {
                Name = "New Product",
                Logo = "http://example.com/logo.png",
                Background = "http://example.com/background.png",
                TotalRating = 10,
                Genre = "Strategy"
            };

            // Act
            var resultDto = await _gameService.CreateProductAsync(newProductDto);

            // Assert
            Assert.NotNull(resultDto);
            Assert.Equal("New Product", resultDto.Name);
            Assert.Equal("http://example.com/logo.png", resultDto.Logo);
            Assert.Equal("http://example.com/background.png", resultDto.Background);
            Assert.Equal(10, resultDto.TotalRating);
            Assert.Equal("Strategy", resultDto.Genre);

            var addedProduct = await _context.Products.FirstOrDefaultAsync(p => p.Name == "New Product");

            Assert.NotNull(addedProduct);
            Assert.Equal("http://example.com/logo.png", addedProduct.Logo);
            Assert.Equal("http://example.com/background.png", addedProduct.Background);
            Assert.Equal(10, addedProduct.TotalRating);
            Assert.Equal("Strategy", addedProduct.Genre);
        }

        [Fact]
        public async Task UpdateProductAsync_UpdatesProduct_WhenProductExists()
        {
            // Arrange
            var updateProductDto = new UpdateProductDto
            {
                Id = 1,
                Name = "Updated Name",
            };

            // Act
            var resultDto = await _gameService.UpdateProductAsync(updateProductDto);

            // Assert
            Assert.NotNull(resultDto);
            Assert.Equal("Updated Name", resultDto.Name);

            var updatedProduct = await _context.Products.FindAsync(1);

            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Name", updatedProduct.Name);
        }

        [Fact]
        public async Task UpdateProductAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var nonExistentProductDto = new UpdateProductDto
            {
                Id = 999,
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiExceptionBase>(
                () => _gameService.UpdateProductAsync(nonExistentProductDto));

            Assert.Equal("Not Found", exception.Message);
        }

        [Fact]
        public async Task DeleteProductAsync_ReturnsTrue_WhenProductExists()
        {
            // Arrange
            var existingProductId = 1;

            // Act
            var result = await _gameService.DeleteProductAsync(existingProductId);

            // Assert
            Assert.True(result);

            var deletedProduct = await _context.Products.FindAsync(existingProductId);

            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task DeleteProductAsync_ThrowsNotFoundException_WhenProductDoesNotExist()
        {
            // Arrange
            var nonExistentProductId = 999;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiExceptionBase>(
                () => _gameService.DeleteProductAsync(nonExistentProductId));

            Assert.Equal("Not Found", exception.Message);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_ReturnsTop3Platforms()
        {
            // Act
            var result = await _gameService.GetTopPlatformsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_CorrectPlatformsAndCounts()
        {
            // Act
            var result = await _gameService.GetTopPlatformsAsync();

            // Assert
            Assert.Equal("PC", result[0].PlatformName);
            Assert.Equal(4, result[0].Count);

            Assert.Equal("Nintendo", result[1].PlatformName);
            Assert.Equal(4, result[1].Count);

            Assert.Equal("Xbox", result[2].PlatformName);
            Assert.Equal(3, result[2].Count);
        }

        [Fact]
        public async Task GetTopPlatformsAsync_CorrectOrdering()
        {
            // Act
            var result = await _gameService.GetTopPlatformsAsync();

            // Assert
            Assert.True(result[0].Count >= result[1].Count);
            Assert.True(result[1].Count >= result[2].Count);
        }

        [Fact]
        public async Task SearchGamesAsync_ValidParameters_ReturnsMatchingGames()
        {
            // Arrange
            string searchTerm = "The";
            int limit = 5;
            int offset = 0;

            // Act
            var result = await _gameService.SearchGamesAsync(searchTerm, limit, offset);

            // Assert
            Assert.NotNull(result);
            int expectedCount = 5;

            Assert.Equal(expectedCount, result.Count);
            Assert.All(result, game => Assert.Contains(searchTerm, game.Name));
        }

        [Fact]
        public async Task SearchGamesAsync_EmptyResultHandling()
        {
            // Act
            var result = await _gameService.SearchGamesAsync("Missing Game", 10, 0);

            // Assert
            Assert.Empty(result);
        }
    }
}
