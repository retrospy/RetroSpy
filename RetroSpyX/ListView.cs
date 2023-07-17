using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

public class ListView<T> : IList, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    public T? SelectedItem { get; set; }

    private readonly IList<T> _list = new List<T>();
    private Window? _window;
    private string? _controlName;

    public void StoreControl(Window sw, string controlName)
    {
        _window = sw;
        _controlName = controlName;
    }

    public T this[int index] { get => _list[index]; set => _list[index] = value; }
    object? IList.this[int index] { get => Count > 0 ? _list[index] : null; set { if (value != null) _list[index] = (T)value; } }


    public int Count => _list.Count;

    public bool IsReadOnly => _list.IsReadOnly;

    public bool IsFixedSize => ((IList)_list).IsFixedSize;

    public bool IsSynchronized => ((IList)_list).IsSynchronized;

    public object SyncRoot => ((IList)_list).SyncRoot;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdateContents(IEnumerable<T>? items)
    {
        if (items == null)
            return;

        T? savedItem = SelectedItem;
        Clear();
        AddRange(items);
        for (int i = 0; i < _list.Count; ++i)
        {
            if (savedItem != null && savedItem.Equals(_list[i]))
            {
                SelectId(i);
                return;
            }
        }
        SelectFirst();
    }

    public void SelectFirst()
    {
        if (_list.Count > 0)
        {
            SelectedItem = _list[0];

            if (Dispatcher.UIThread.CheckAccess())
            {
                SelectingItemsControl? c = _window?.FindControl<SelectingItemsControl>(_controlName ?? String.Empty);
                if (c != null)
                {
                    c.SelectedIndex = 0;
                }
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    SelectingItemsControl? c = _window?.FindControl<SelectingItemsControl>(_controlName ?? String.Empty);
                    if (c != null)
                    {
                        c.SelectedIndex = 0;
                    }
                });
            }
        }
    }

    public void SelectId(int id)
    {
        if (_list.Count > 0 && id >= 0 && id < _list.Count)
        {
            SelectedItem = _list[id];

            if (Dispatcher.UIThread.CheckAccess())
            {
                SelectingItemsControl? c = _window?.FindControl<SelectingItemsControl>(_controlName ?? String.Empty);
                if (c != null)
                {
                    c.SelectedIndex = id;
                }
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    SelectingItemsControl? c = _window?.FindControl<SelectingItemsControl>(_controlName ?? String.Empty);
                    if (c != null)
                    {
                        c.SelectedIndex = id;
                    }
                });
            }

        }
        else
        {
            SelectFirst();
        }
    }

    public void SelectIdFromText(T text)
    {
        int index = _list.IndexOf(text);
        SelectId(index);
    }

    public int GetSelectedId()
    {
        return SelectedItem != null ? _list.IndexOf(SelectedItem) : -1;
    }

    public void Add(T item)
    {
        _list.Add(item);

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
    }

    public int Add(object? value)
    {
        if (value != null)
        {
            _list.Add((T)value);

            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
        }

        return _list.Count;
    }

    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    public void Clear()
    {
        _list.Clear();

        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
    }

    public bool Contains(T item)
    {
        return _list.Contains(item);
    }

    public bool Contains(object? value)
    {
        if (value == null)
            return false;

        return Contains((T)value);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _list.CopyTo(array, arrayIndex);
    }

    public void CopyTo(Array array, int index)
    {
        CopyTo((T[])array, index);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return _list.GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return _list.IndexOf(item);
    }

    public int IndexOf(object? value)
    {
        if (value == null)
            return -1;

        return IndexOf((T)value);
    }

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
    }

    public void Insert(int index, object? value)
    {
        if (value != null)
            Insert(index, (T)value);
    }

    public bool Remove(T item)
    {
        return _list.Remove(item);
    }

    public void Remove(object? value)
    {
        if (value != null)
            Remove((T)value);
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _list.GetEnumerator();
    }
}