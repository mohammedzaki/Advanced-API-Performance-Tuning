package com.example.demo.service;

import com.example.demo.grpc.ProductGrpc;
import com.example.demo.grpc.ProductProto.*;
import io.grpc.ManagedChannel;
import io.grpc.ManagedChannelBuilder;
import org.springframework.stereotype.Service;

import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import java.util.List;

@Service
public class ProductGrpcClientService {
    
    private ManagedChannel channel;
    private ProductGrpc.ProductBlockingStub productStub;
    
    @PostConstruct
    public void init() {
        // Connect to the .NET gRPC service running in Docker
        // Use Docker service name 'dotnet-app' and port 8085 (internal container port)
        channel = ManagedChannelBuilder.forAddress("dotnet-app", 8085)
                .usePlaintext()
                .build();
        
        productStub = ProductGrpc.newBlockingStub(channel);
    }
    
    @PreDestroy
    public void shutdown() {
        if (channel != null) {
            channel.shutdown();
        }
    }
    
    public ProductResponse getProductById(int id) {
        ProductRequest request = ProductRequest.newBuilder()
                .setId(id)
                .build();
        
        return productStub.getProduct(request);
    }
    
    public List<ProductResponse> getAllProducts() {
        Empty request = Empty.newBuilder().build();
        ProductListResponse response = productStub.getProducts(request);
        return response.getProductsList();
    }
}