using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TodoApi;

[DebuggerDisplay($"{nameof(GetDebuggerDisplay)}(),nq}}")]
public class Service
{
    private readonly ToDoDbContext _context;
    private readonly IConfiguration _configuration;

    public Service(ToDoDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
        var baseUrl = _configuration["ApiSettings:BaseUrl"];
    }
    public async Task<IEnumerable<Item?>> GetItems()
    {
        return await _context.Items.ToListAsync();
    }
    public async Task<string?> AddItems(Item item)
    {
        try
        {
            if (item == null) return "Item is null";

            // לא לשלוח את ה-id כי הוא יתמלא אוטומטית
            _context.Items.Add(item);
            await _context.SaveChangesAsync();  // שמור את השינויים

            return "Item added successfully";
        }
        catch (Exception ex)
        {
            return $"There is a problem: {ex.Message}";
        }
    }


    public async Task<string?> UpdateItems(Item NewItem)
    {
        try
        {
            // חיפוש הפריט לפי ה-ID
            var item = await _context.Items.FindAsync(NewItem.Id);
            if (item == null) return "Item not found";

            // עדכון השדות של item עם הערכים החדשים מ-NewItem
            item.Name = NewItem.Name;
            item.IsComplete = NewItem.IsComplete;

            // שמירת השינויים במסד הנתונים
            await _context.SaveChangesAsync();

            return "Updated!";
        }
        catch (Exception ex)
        {
            // החזרת הודעת שגיאה במקרה של בעיה
            return $"Not updated, there is a problem: {ex.Message}";
        }
    }


    public async Task<string?> delete(int id)
    {
        try
        {
            Item? item = await _context.Items.FindAsync(id);
            if (item == null)
                return ("this item is not exist");
            _context.Remove(item);
            await _context.SaveChangesAsync();
            return "Deleted successfully";

        }
        catch
        {
            return "Not deleted, there is a problem";
        }
    }

    private string? GetDebuggerDisplay()
    {
        return ToString();
    }
}
