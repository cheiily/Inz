using System;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Random = UnityEngine.Random;

public class Counter : MonoBehaviour {
    public int size = 4;
    public int numCustomers = 0;
    public List<GameObject> anchors;
    public GameObject customerPrefab;
    public List<Sprite> customerSprites;

    public CustomerInstance[] customers;

    private void Awake() {
        customers = new CustomerInstance[size];
    }

    public bool CanAdd() {
        return numCustomers < size;
    }

    public void AddCustomer(CustomerPreset preset) {
        int index = FindFreeSeat();
        var customer = Instantiate(customerPrefab, anchors[ index ].transform);
        var customerInstance = customer.GetComponent<CustomerInstance>();
        customerInstance.preset = preset;
        customerInstance.sprite = customerSprites[Random.Range(0, customerSprites.Count)];
        customers[index] = customer.GetComponent<CustomerInstance>();
        numCustomers++;
    }

    public void RemoveCustomer(CustomerInstance customer) {
        int idx = Array.IndexOf(customers, customer);
        customers[idx] = null;
        numCustomers--;
    }

    public int FindFreeSeat() {
        for (int i = 0; i < size; i++) {
            if (customers[i] == null) {
                return i;
            }
        }
        return -1;
    }
}
