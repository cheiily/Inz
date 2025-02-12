using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using InzGame.DisplayHandlers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Counter : MonoBehaviour {
    public int size = 4;
    public int numCustomers = 0;
    public List<GameObject> anchors;
    public GameObject customerPrefab;
    public List<Sprite> customerSprites;

    public CustomerInstance[] customers;
    public GameConfiguration _config;

    private void Awake() {
        _config = GetComponent<GameManager>().config;
        customers = new CustomerInstance[size];

        foreach (var anchor in anchors) {
            anchor.gameObject.SetActive(false);
        }
    }

    public bool CanAdd() {
        return numCustomers < size;
    }

    public void AddCustomer(CustomerPreset preset) {
        int index = FindFreeSeat();
        var customer = Instantiate(customerPrefab, anchors[ index ].transform);

        var customerInstance = customer.GetComponent<CustomerInstance>();
        customerInstance.OnCustomerRemove += RemoveCustomer;
        customerInstance.preset = preset;

        var customerDisplay = customer.GetComponent<CustomerDisplay>();
        customerDisplay.personImage.sprite = customerSprites[Random.Range(0, customerSprites.Count)];
        customerDisplay.orderImage.sprite = _config.elementProperties.GetFor(preset.order).sprite_order_bubble;
        customers[index] = customerInstance;
        numCustomers++;
        customer.transform.parent.gameObject.SetActive(true);

        Debug.Log("Customer added; numCustomers: " + numCustomers);
    }

    public void RemoveCustomer(CustomerInstance customer) {
        int idx = Array.IndexOf(customers, customer);
        customers[idx] = null;
        numCustomers--;

        anchors[idx].gameObject.SetActive(false);

        Debug.Log("Customer removed; numCustomers: " + numCustomers);
    }

    public int FindFreeSeat() {
        for (int i = 0; i < size; i++) {
            if (customers[i] == null) {
                return i;
            }
        }
        return -1;
    }

    public void RemoveCustomer(object sender, EventArgs _) {
        RemoveCustomer((CustomerInstance) sender);
    }
}
