using System;
using System.Collections.Generic;
using UnityEngine;

namespace InzGame {
    public class CustomerEvaluation : MonoBehaviour {
        public enum Method {
            Linear,
            Step,
            WeightedThresholds,
            LinearPadded,
            StepPadded,
            WeightedThresholdsPadded
        }

        public static Dictionary<Method, Func<CustomerInstance, float>> methods;
        static CustomerEvaluation() {
            methods = new Dictionary<Method, Func<CustomerInstance, float>> {
                { Method.Linear, Linear },
                { Method.Step, Step },
                { Method.WeightedThresholds, WeightedThresholds },
                { Method.LinearPadded, LinearPadded },
                { Method.StepPadded, StepPadded },
                { Method.WeightedThresholdsPadded, WeightedThresholdsPadded }
            };
        }

        public static float Invoke(Method method, CustomerInstance instance) {
            return methods[ method ].Invoke(instance);
        }

        public static float Linear(CustomerInstance instance) {
            return _Linear(instance);
        }

        public static float LinearPadded(CustomerInstance instance) {
            return _Linear(instance, 1);
        }

        public static float Step(CustomerInstance instance) {
            return _Step(instance);
        }

        public static float StepPadded(CustomerInstance instance) {
            return _Step(instance, 1);
        }

        public static float WeightedThresholds(CustomerInstance instance) {
            return _WeightedThresholds(instance);
        }

        public static float WeightedThresholdsPadded(CustomerInstance instance) {
            return _WeightedThresholds(instance, 1);
        }

        private static float _Linear(CustomerInstance customer, int padding = 0) {
            float perStep = 100.0f / (customer.thresholds.Count + padding);
            float linear = customer.currentThreshold >= customer.thresholds.Count
                ? 0
                : customer.timeInThreshold / customer.thresholds[ customer.currentThreshold ];
            linear *= perStep;
            float step = customer.currentThreshold * perStep;
            return 100 - step - linear;
        }

        private static float _Step(CustomerInstance customer, int padding = 0) {
            return 100 - customer.currentThreshold * (100.0f / (customer.thresholds.Count + padding));
        }

        private static float _WeightedThresholds(CustomerInstance customer, int padding = 0) {
            float perStep = 100.0f / (customer.thresholds.Count + padding);
            float linear = customer.currentThreshold >= customer.thresholds.Count
                ? 0
                : customer.timeInThreshold / customer.thresholds[ customer.currentThreshold ];
            float step = customer.currentThreshold * perStep;
            linear *= perStep * 3/4;
            return 100 - step - linear;
        }
    }
}