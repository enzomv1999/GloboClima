using FluentAssertions;
using GloboClima.API.DTOs;
using GloboClima.API.Validators;
using Xunit;

namespace GloboClima.API.UnitTests.Tests
{
    public class FavoriteInputValidatorTests
    {
        [Fact]
        public void Validate_Should_Pass_For_Valid_Input()
        {
            var validator = new FavoriteInputValidator();
            var input = new FavoriteInput { Type = "city", Name = "Sao Paulo" };

            var result = validator.Validate(input);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_Should_Fail_For_Invalid_Type()
        {
            var validator = new FavoriteInputValidator();
            var input = new FavoriteInput { Type = "foo", Name = "Any" };

            var result = validator.Validate(input);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(FavoriteInput.Type));
        }

        [Fact]
        public void Validate_Should_Fail_For_Empty_Name_And_TooLong_Name()
        {
            var validator = new FavoriteInputValidator();
            var emptyName = new FavoriteInput { Type = "city", Name = string.Empty };
            var longName = new FavoriteInput { Type = "city", Name = new string('a', 101) };

            validator.Validate(emptyName).IsValid.Should().BeFalse();
            validator.Validate(longName).IsValid.Should().BeFalse();
        }
    }
}
