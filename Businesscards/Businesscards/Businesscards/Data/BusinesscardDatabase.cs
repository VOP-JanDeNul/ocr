using Businesscards.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Businesscards.Data
{
    public class BusinesscardDatabase
    {
        readonly SQLiteAsyncConnection database;

        public BusinesscardDatabase(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
            database.CreateTableAsync<Businesscard>().Wait();
        }

        public Task<List<Businesscard>> GetBusinesscardsAsync()
        {
            //Get all businesscards.
            return database.Table<Businesscard>().ToListAsync();
        }

        public Task<Businesscard> GetBusinesscardAsync(int id)
        {
            // Get a specific businesscard.
            return database.Table<Businesscard>()
                            .Where(i => i.Id == id)
                            .FirstOrDefaultAsync();
        }

        public Task<int> SaveBusinesscardAsync(Businesscard card)
        {
            if (card.Id != 0)
            {
                // Update an existing businesscard.
                return database.UpdateAsync(card);
            }
            else
            {
                // Save a new businesscard.
                return database.InsertAsync(card);
            }
        }

        public Task<int> DeleteBusinesscardAsync(Businesscard card)
        {
            // Delete a businesscard.
            return database.DeleteAsync(card);
        }
    }
}