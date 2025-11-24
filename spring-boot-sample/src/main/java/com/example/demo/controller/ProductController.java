package com.example.demo.controller;

import com.example.demo.model.Product;
import com.example.demo.service.ProductService;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RestController;

import java.util.List;

@RestController
public class ProductController {

    private final ProductService svc;

    public ProductController(ProductService svc) {
        this.svc = svc;
    }

    @GetMapping("/api/products")
    public List<Product> getProducts() {
        return svc.getAll();
    }

    @GetMapping("/api/products-delayed")
    public List<Product> getProductsDelayed() {
        return svc.getAllDelayed();
    }

    @GetMapping("/api/products/{id}")
    public Product getProduct(@PathVariable long id) {
        return svc.getById(id);
    }

    @GetMapping("/actuator/health")
    public String health() {
        return "UP";
    }
}
