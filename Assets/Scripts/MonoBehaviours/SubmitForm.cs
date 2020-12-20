using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SubmitForm : MonoBehaviour
{
    [SerializeField]
    private List<InputField> _inputFields;

    [SerializeField]
    private List<Dropdown> _inputDropDowns;

    [SerializeField]
    private UnityEvent _submitForm;

    public void Submit()
    {
        _submitForm.Invoke();
    }

    public void SubmitTransactionTracking()
    {
        Dictionary<AddressAndChain, List<TransactionIdAndType>> transactions = new Dictionary<AddressAndChain, List<TransactionIdAndType>>();

        AddressAndChain addressAndChain = new AddressAndChain(_inputFields.Find(f => f.name == "Address").text,
            (BlockChain)Enum.Parse(typeof(BlockChain), _inputDropDowns.Find(d => d.name == "Blockchain").itemText.text));

        TransactionIdAndType transactionIdAndType = new TransactionIdAndType(_inputFields.Find(f => f.name == "TransactionHash").text,
            (TransactionType)Enum.Parse(typeof(TransactionType), _inputDropDowns.Find(d => d.name == "TransactionType").itemText.text), 20);

        transactions.Add(addressAndChain, new List<TransactionIdAndType> { transactionIdAndType });

        MainComponent.Instance.AddTransactionTracking(new TransactionTracking(transactions));
    }
}
