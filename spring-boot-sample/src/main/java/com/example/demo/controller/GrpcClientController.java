package com.example.demo.controller;

import com.example.demo.dto.ProductDto;
import com.example.demo.grpc.ProductProto.ProductResponse;
import com.example.demo.service.ProductGrpcClientService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;
import java.util.stream.Collectors;

@RestController
@RequestMapping("/api/grpc")
public class GrpcClientController {
    
    @Autowired
    private ProductGrpcClientService grpcClientService;
    
    @GetMapping("/products")
    public List<ProductDto> getProducts() {
        List<ProductResponse> grpcResponses = grpcClientService.getAllProducts();
        return grpcResponses.stream()
                .map(this::convertToDto)
                .collect(Collectors.toList());
    }
    
    @GetMapping("/products/{id}")
    public ProductDto getProduct(@PathVariable int id) {
        ProductResponse grpcResponse = grpcClientService.getProductById(id);
        return convertToDto(grpcResponse);
    }

    @GetMapping("/health")
    public String health() {
        try {
            List<ProductResponse> grpcResponses = grpcClientService.getAllProducts();
            if (grpcResponses != null) {
                return "UP";
            }
            return "DOWN";
        } catch (Exception e) {
            return "DOWN";
        }
    }
    
    private ProductDto convertToDto(ProductResponse grpcResponse) {
        return new ProductDto(
            grpcResponse.getId(),
            grpcResponse.getName(),
            grpcResponse.getPrice(),
            grpcResponse.getDescription()
        );
    }
}