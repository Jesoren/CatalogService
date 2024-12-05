namespace Models;

public class Bid
{
    public Guid Id { get; set; } // Unik ID for buddet
    public Guid UserId { get; set; } // Referencer til brugeren, der afgiver buddet
    public int Amount { get; set; } // Beløbet for buddet
    public DateTime Timestamp { get; set; } // Tidspunkt for buddet
    public string? Name { get; set; }
}