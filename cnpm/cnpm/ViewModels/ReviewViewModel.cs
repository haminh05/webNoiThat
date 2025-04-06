using System;
using System.ComponentModel.DataAnnotations;

namespace cnpm.ViewModels
{
    public class ReviewViewModel
    {
        public int ProductID { get; set; }

        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Bình luận không quá 1000 ký tự.")]
        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.Now;
    }
}
