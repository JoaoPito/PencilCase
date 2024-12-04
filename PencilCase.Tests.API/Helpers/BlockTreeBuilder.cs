using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PencilCase.Shared.Models.Notebooks;

namespace PencilCase.Tests.API.Helpers;

public static class BlockTreeBuilder
{
    public static Block AddTopic(this Block root, string name)
    {
        foreach (var child in root.Children)
            if (child.Name == name) return child;
        
        var topic = new Block()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Parent = root,
            ParentId = root.Id
        };
        root.Children.Add(topic);
        return topic;
    }

    public static Block AddNotebook(this Block root, string name)
    {
        foreach (var child in root.Children)
            if (child.Name == name) return child;
        
        var notebook = new Block()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Parent = root,
            ParentId = root.Id,
            Type = BlockType.Notebook
        };
        root.Children.Add(notebook);
        return notebook;
    }

    public static Block AddDocument(this Block root, string name)
    {
        foreach (var child in root.Children)
            if (child.Name == name) return child;
        
        var document = new Block()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Parent = root,
            ParentId = root.Id,
            Type = BlockType.Source
        };
        root.Children.Add(document);
        return document;
    }

    public static Block AddQuestion(this Block root, string question, int order, List<string>? answers = null)
    {
        if (root.Type != BlockType.Notebook)
            throw new ArgumentException("Invalid root block type");
        
        foreach (var child in root.Children)
            if (child.Name == question) return child;

        var questionCell = new Block()
        {
            Id = Guid.NewGuid(),
            Name = question,
            Parent = root,
            ParentId = root.Id,
            Type = BlockType.Cell,
            Children = new List<Block>(),
            Properties = new BlockProperties()
            {
                CellType = CellType.Question,
                Order = order
            }
        };

        if (answers != null)
        {
            foreach (var answer in answers)
            {
                questionCell.AddAnswer(answer, 0);
            }
        }

        root.Children.Add(questionCell);
        return questionCell;
    }
    
    public static Block AddAnswer(this Block question, string contents, int order)
    {
        if (question.Type != BlockType.Cell || question.Properties!.CellType != CellType.Question)
            throw new ArgumentException("Invalid root block type");
        
        var answer = new Block()
        {
            Id = Guid.NewGuid(),
            Name = contents,
            Parent = question,
            ParentId = question.Id,
            Type = BlockType.Cell,
            Properties = new BlockProperties()
            {
                CellType = CellType.Text,
                Order = order
            }
        };
        question.Children.Add(answer);
        return answer;
    }
    
    public static Block AddChunk(this Block document, string contents, int order)
    {
        if (document.Type != BlockType.Source)
            throw new ArgumentException("Invalid root block type");
        
        var chunk = new Block()
        {
            Id = Guid.NewGuid(),
            Name = contents,
            Parent = document,
            ParentId = document.Id,
            Type = BlockType.Cell,
            Properties = new BlockProperties()
            {
                CellType = CellType.Text,
                Order = order
            }
        };
        document.Children.Add(chunk);
        return chunk;
    }
}