#!/usr/bin/env python3
"""Generate minimal ONNX model for Intentum.Tests: input [1,2] float, output [1,3] float (MatMul)."""
import os
import sys

try:
    import onnx
    from onnx import helper, TensorProto
except ImportError:
    print("pip install onnx", file=sys.stderr)
    sys.exit(1)

# Input [1,2], weights [2,3], output [1,3]
input_name = "input"
output_name = "output"
w_name = "W"

input_shape = [1, 2]
output_shape = [1, 3]
w_shape = [2, 3]
# Weights so that output is deterministic
w_data = [1.0, 0.0, 0.0, 0.0, 1.0, 0.0]  # first col [1,0], second [0,1] -> output cols

input_tensor = helper.make_tensor_value_info(input_name, TensorProto.FLOAT, input_shape)
output_tensor = helper.make_tensor_value_info(output_name, TensorProto.FLOAT, output_shape)
w_initializer = helper.make_tensor(w_name, TensorProto.FLOAT, w_shape, w_data)

matmul = helper.make_node("MatMul", [input_name, w_name], [output_name])
graph = helper.make_graph([matmul], "minimal_intent", [input_tensor], [output_tensor], [w_initializer])
model = helper.make_model(graph, opset_imports=[helper.make_opsetid("", 14)])
onnx.checker.check_model(model)

out_dir = os.path.join(os.path.dirname(__file__), "..", "tests", "Intentum.Tests", "fixtures")
os.makedirs(out_dir, exist_ok=True)
out_path = os.path.join(out_dir, "minimal_intent.onnx")
onnx.save(model, out_path)
print(out_path)
