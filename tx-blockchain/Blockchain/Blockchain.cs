using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

// this class implements the actual linked list
public class Blockchain
{
    private string _filename;

    public string Filename { get { return _filename; } }

    public IList<Block> Chain { set; get; } = new List<Block>();
    public int Difficulty { set; get; } = 2;

    public Blockchain() {}

    public Blockchain(bool genesis = false)
    {
        if (genesis == true)
            AddGenesisBlock(); // add the first block
    }

    public Blockchain(string filename)
    {
        _filename = filename;

        if (System.IO.File.Exists(filename))
        {
            string bc = System.IO.File.ReadAllText(filename);
            this.Chain = JsonConvert.DeserializeObject<Blockchain>(bc).Chain;
        }
        else
            AddGenesisBlock();
    }

    public void Save()
    {
        System.IO.FileInfo file = new System.IO.FileInfo(this.Filename);
        file.Directory.Create();

        // store the blockchain as a file
        System.IO.File.WriteAllText(this.Filename,
            JsonConvert.SerializeObject(this));
    }

    public Block CreateGenesisBlock()
    {
        return new Block(DateTime.Now, null, "{}");
    }

    public void AddGenesisBlock()
    {
        Chain.Add(CreateGenesisBlock());
    }

    // return the last block in the list
    public Block GetCurrentBlock()
    {
        return Chain[Chain.Count - 1];
    }

    public Block GetBlock(string blockHash)
    {
        return Chain.FirstOrDefault(h => h.BlockHash == blockHash);
    }

    // adds a new block to the chain
    public bool AddBlock(Block block)
    {
        // check blockchain consistency first
        if (this.IsValid() == true)
        {
            Block latestBlock = GetCurrentBlock();
            block.Index = latestBlock.Index + 1;
            block.PreviousBlockHash = latestBlock.BlockHash;

            // mine for a valid hash
            block.Mine(this.Difficulty);

            // add the block to the chain
            Chain.Add(block);
            return true;
        }
        return false;
    }

    // checks, if the blockchain is consistent by
    // re-generating and comparing the hashes in each block
    public bool IsValid(string blockHash = "")
    {
        // check all blocks
        int iNumberBlocks = Chain.Count;

        // check all blocks until the given, optional hash
        if (blockHash != "")
        {
            Block block = Chain.FirstOrDefault(h => h.BlockHash == blockHash);
            if (block != null)
            {
                iNumberBlocks = block.Index + 1;
            }
        }

        // loop through all blocks, generate their hashes and compare
        for (int i = 1; i < iNumberBlocks; i++)
        {
            Block currentBlock = Chain[i];
            Block previousBlock = Chain[i - 1];

            if (currentBlock.BlockHash != currentBlock.GenerateBlockHash())
            {
                return false;
            }

            if (currentBlock.PreviousBlockHash != previousBlock.BlockHash)
            {
                return false;
            }
        }

        return true;
    }
}
