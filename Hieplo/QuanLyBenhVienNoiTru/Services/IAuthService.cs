using System.Threading.Tasks;
using QuanLyBenhVienNoiTru.Models.ViewModels;

namespace QuanLyBenhVienNoiTru.Services
{
    public interface IAuthService
    {
        Task<bool> ValidateUserAsync(LoginViewModel model);
        Task<bool> RegisterUserAsync(RegisterViewModel model);
    }
}